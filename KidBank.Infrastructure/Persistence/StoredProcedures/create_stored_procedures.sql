-- ============================================
-- KidBank - Stored Procedures for PostgreSQL
-- ============================================

-- 1. Transfer between accounts (atomic operation with optimistic locking)
CREATE OR REPLACE FUNCTION sp_transfer_between_accounts(
    p_source_account_id UUID,
    p_destination_account_id UUID,
    p_amount DECIMAL(18,2),
    p_description TEXT,
    p_created_by_id UUID
)
RETURNS TABLE(
    success BOOLEAN,
    error_message TEXT,
    transaction_id UUID
) AS $$
DECLARE
    v_source_balance DECIMAL(18,2);
    v_source_version INT;
    v_dest_version INT;
    v_source_user_id UUID;
    v_dest_user_id UUID;
    v_source_currency VARCHAR(3);
    v_dest_currency VARCHAR(3);
    v_transaction_id UUID;
    v_rows_affected INT;
BEGIN
    -- Lock accounts for update (prevent race conditions)
    SELECT balance, version, user_id, currency INTO v_source_balance, v_source_version, v_source_user_id, v_source_currency
    FROM accounts
    WHERE id = p_source_account_id AND is_active = true
    FOR UPDATE;
    
    IF NOT FOUND THEN
        RETURN QUERY SELECT false, 'Source account not found or inactive'::TEXT, NULL::UUID;
        RETURN;
    END IF;
    
    SELECT version, user_id, currency INTO v_dest_version, v_dest_user_id, v_dest_currency
    FROM accounts
    WHERE id = p_destination_account_id AND is_active = true
    FOR UPDATE;
    
    IF NOT FOUND THEN
        RETURN QUERY SELECT false, 'Destination account not found or inactive'::TEXT, NULL::UUID;
        RETURN;
    END IF;
    
    -- Validate same currency
    IF v_source_currency != v_dest_currency THEN
        RETURN QUERY SELECT false, 'Currency mismatch between accounts'::TEXT, NULL::UUID;
        RETURN;
    END IF;
    
    -- Check sufficient balance
    IF v_source_balance < p_amount THEN
        RETURN QUERY SELECT false, 'Insufficient funds'::TEXT, NULL::UUID;
        RETURN;
    END IF;
    
    -- Validate amount
    IF p_amount <= 0 THEN
        RETURN QUERY SELECT false, 'Amount must be positive'::TEXT, NULL::UUID;
        RETURN;
    END IF;
    
    -- Generate transaction ID
    v_transaction_id := gen_random_uuid();
    
    -- Debit source account with optimistic locking
    UPDATE accounts 
    SET balance = balance - p_amount,
        version = version + 1,
        updated_at = NOW()
    WHERE id = p_source_account_id AND version = v_source_version;
    
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;
    IF v_rows_affected = 0 THEN
        RETURN QUERY SELECT false, 'Concurrency conflict on source account'::TEXT, NULL::UUID;
        RETURN;
    END IF;
    
    -- Credit destination account with optimistic locking
    UPDATE accounts 
    SET balance = balance + p_amount,
        version = version + 1,
        updated_at = NOW()
    WHERE id = p_destination_account_id AND version = v_dest_version;
    
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;
    IF v_rows_affected = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on destination account';
    END IF;
    
    -- Create transaction record (immutable ledger entry)
    INSERT INTO transactions (
        id, account_id, type, amount, currency, balance_after,
        description, reference_id, created_at
    ) VALUES (
        v_transaction_id, p_source_account_id, 1, -p_amount, v_source_currency,
        v_source_balance - p_amount, p_description, p_destination_account_id, NOW()
    );
    
    INSERT INTO transactions (
        id, account_id, type, amount, currency, balance_after,
        description, reference_id, created_at
    ) VALUES (
        gen_random_uuid(), p_destination_account_id, 0, p_amount, v_dest_currency,
        (SELECT balance FROM accounts WHERE id = p_destination_account_id),
        p_description, p_source_account_id, NOW()
    );
    
    RETURN QUERY SELECT true, NULL::TEXT, v_transaction_id;
END;
$$ LANGUAGE plpgsql;


-- 2. Approve task and pay reward (atomic)
CREATE OR REPLACE FUNCTION sp_approve_task_and_pay_reward(
    p_task_id UUID,
    p_approver_id UUID
)
RETURNS TABLE(
    success BOOLEAN,
    error_message TEXT,
    new_balance DECIMAL(18,2)
) AS $$
DECLARE
    v_task RECORD;
    v_parent_account_id UUID;
    v_kid_account_id UUID;
    v_parent_balance DECIMAL(18,2);
    v_parent_version INT;
    v_kid_version INT;
    v_new_kid_balance DECIMAL(18,2);
    v_rows_affected INT;
BEGIN
    -- Get task details
    SELECT t.*, u.family_id as kid_family_id
    INTO v_task
    FROM task_assignments t
    JOIN users u ON t.assigned_to_id = u.id
    WHERE t.id = p_task_id
    FOR UPDATE;
    
    IF NOT FOUND THEN
        RETURN QUERY SELECT false, 'Task not found'::TEXT, 0::DECIMAL(18,2);
        RETURN;
    END IF;
    
    IF v_task.status != 2 THEN -- 2 = Completed
        RETURN QUERY SELECT false, 'Task is not in completed status'::TEXT, 0::DECIMAL(18,2);
        RETURN;
    END IF;
    
    -- Get parent's main account
    SELECT id, balance, version INTO v_parent_account_id, v_parent_balance, v_parent_version
    FROM accounts
    WHERE user_id = p_approver_id AND type = 0 AND is_active = true
    FOR UPDATE;
    
    IF NOT FOUND THEN
        RETURN QUERY SELECT false, 'Parent account not found'::TEXT, 0::DECIMAL(18,2);
        RETURN;
    END IF;
    
    -- Get kid's main account
    SELECT id, version INTO v_kid_account_id, v_kid_version
    FROM accounts
    WHERE user_id = v_task.assigned_to_id AND type = 0 AND is_active = true
    FOR UPDATE;
    
    IF NOT FOUND THEN
        RETURN QUERY SELECT false, 'Kid account not found'::TEXT, 0::DECIMAL(18,2);
        RETURN;
    END IF;
    
    -- Check parent has enough funds
    IF v_parent_balance < v_task.reward_amount THEN
        RETURN QUERY SELECT false, 'Insufficient funds for reward'::TEXT, 0::DECIMAL(18,2);
        RETURN;
    END IF;
    
    -- Update task status
    UPDATE task_assignments
    SET status = 3, -- Approved
        approved_at = NOW()
    WHERE id = p_task_id;
    
    -- Debit parent
    UPDATE accounts 
    SET balance = balance - v_task.reward_amount,
        version = version + 1,
        updated_at = NOW()
    WHERE id = v_parent_account_id AND version = v_parent_version;
    
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;
    IF v_rows_affected = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on parent account';
    END IF;
    
    -- Credit kid
    UPDATE accounts 
    SET balance = balance + v_task.reward_amount,
        version = version + 1,
        updated_at = NOW()
    WHERE id = v_kid_account_id AND version = v_kid_version
    RETURNING balance INTO v_new_kid_balance;
    
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;
    IF v_rows_affected = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on kid account';
    END IF;
    
    -- Create transaction records
    INSERT INTO transactions (id, account_id, type, amount, currency, balance_after, description, created_at)
    VALUES (
        gen_random_uuid(), v_parent_account_id, 1, -v_task.reward_amount, 'RUB',
        v_parent_balance - v_task.reward_amount, 'Task reward: ' || v_task.title, NOW()
    );
    
    INSERT INTO transactions (id, account_id, type, amount, currency, balance_after, description, created_at)
    VALUES (
        gen_random_uuid(), v_kid_account_id, 0, v_task.reward_amount, 'RUB',
        v_new_kid_balance, 'Task reward: ' || v_task.title, NOW()
    );
    
    -- Add XP for completing task
    UPDATE users
    SET total_xp = total_xp + 25,
        updated_at = NOW()
    WHERE id = v_task.assigned_to_id;
    
    RETURN QUERY SELECT true, NULL::TEXT, v_new_kid_balance;
END;
$$ LANGUAGE plpgsql;


-- 3. Deposit to savings goal
CREATE OR REPLACE FUNCTION sp_deposit_to_goal(
    p_goal_id UUID,
    p_user_id UUID,
    p_amount DECIMAL(18,2)
)
RETURNS TABLE(
    success BOOLEAN,
    error_message TEXT,
    new_saved_amount DECIMAL(18,2),
    goal_completed BOOLEAN
) AS $$
DECLARE
    v_goal RECORD;
    v_account_id UUID;
    v_account_balance DECIMAL(18,2);
    v_account_version INT;
    v_new_saved DECIMAL(18,2);
    v_is_completed BOOLEAN := false;
    v_rows_affected INT;
BEGIN
    -- Get goal details
    SELECT * INTO v_goal
    FROM wishlist_goals
    WHERE id = p_goal_id
    FOR UPDATE;
    
    IF NOT FOUND THEN
        RETURN QUERY SELECT false, 'Goal not found'::TEXT, 0::DECIMAL(18,2), false;
        RETURN;
    END IF;
    
    IF v_goal.user_id != p_user_id THEN
        RETURN QUERY SELECT false, 'Goal does not belong to user'::TEXT, 0::DECIMAL(18,2), false;
        RETURN;
    END IF;
    
    IF v_goal.status != 0 THEN -- 0 = Active
        RETURN QUERY SELECT false, 'Goal is not active'::TEXT, 0::DECIMAL(18,2), false;
        RETURN;
    END IF;
    
    -- Get user's main account
    SELECT id, balance, version INTO v_account_id, v_account_balance, v_account_version
    FROM accounts
    WHERE user_id = p_user_id AND type = 0 AND is_active = true
    FOR UPDATE;
    
    IF NOT FOUND THEN
        RETURN QUERY SELECT false, 'Account not found'::TEXT, 0::DECIMAL(18,2), false;
        RETURN;
    END IF;
    
    IF v_account_balance < p_amount THEN
        RETURN QUERY SELECT false, 'Insufficient funds'::TEXT, 0::DECIMAL(18,2), false;
        RETURN;
    END IF;
    
    -- Calculate new saved amount
    v_new_saved := v_goal.saved_amount + p_amount;
    
    -- Check if goal is completed
    IF v_new_saved >= v_goal.target_amount THEN
        v_is_completed := true;
    END IF;
    
    -- Debit account
    UPDATE accounts 
    SET balance = balance - p_amount,
        version = version + 1,
        updated_at = NOW()
    WHERE id = v_account_id AND version = v_account_version;
    
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;
    IF v_rows_affected = 0 THEN
        RETURN QUERY SELECT false, 'Concurrency conflict'::TEXT, 0::DECIMAL(18,2), false;
        RETURN;
    END IF;
    
    -- Update goal
    UPDATE wishlist_goals
    SET saved_amount = v_new_saved,
        status = CASE WHEN v_is_completed THEN 1 ELSE 0 END, -- 1 = Completed
        completed_at = CASE WHEN v_is_completed THEN NOW() ELSE NULL END,
        version = version + 1,
        updated_at = NOW()
    WHERE id = p_goal_id;
    
    -- Create transaction
    INSERT INTO transactions (id, account_id, type, amount, currency, balance_after, description, reference_id, created_at)
    VALUES (
        gen_random_uuid(), v_account_id, 1, -p_amount, v_goal.currency,
        v_account_balance - p_amount, 'Deposit to goal: ' || v_goal.title, p_goal_id, NOW()
    );
    
    -- Add XP for saving (10 XP per deposit)
    UPDATE users
    SET total_xp = total_xp + 10,
        updated_at = NOW()
    WHERE id = p_user_id;
    
    -- Extra XP if goal completed (50 XP)
    IF v_is_completed THEN
        UPDATE users
        SET total_xp = total_xp + 50,
            updated_at = NOW()
        WHERE id = p_user_id;
    END IF;
    
    RETURN QUERY SELECT true, NULL::TEXT, v_new_saved, v_is_completed;
END;
$$ LANGUAGE plpgsql;


-- 4. Approve money request and transfer funds
CREATE OR REPLACE FUNCTION sp_approve_money_request(
    p_request_id UUID,
    p_approver_id UUID,
    p_note TEXT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    error_message TEXT,
    new_kid_balance DECIMAL(18,2)
) AS $$
DECLARE
    v_request RECORD;
    v_parent_account_id UUID;
    v_kid_account_id UUID;
    v_parent_balance DECIMAL(18,2);
    v_parent_version INT;
    v_kid_version INT;
    v_new_balance DECIMAL(18,2);
    v_rows_affected INT;
BEGIN
    -- Get request details
    SELECT * INTO v_request
    FROM money_requests
    WHERE id = p_request_id
    FOR UPDATE;
    
    IF NOT FOUND THEN
        RETURN QUERY SELECT false, 'Request not found'::TEXT, 0::DECIMAL(18,2);
        RETURN;
    END IF;
    
    IF v_request.status != 0 THEN -- 0 = Pending
        RETURN QUERY SELECT false, 'Request is not pending'::TEXT, 0::DECIMAL(18,2);
        RETURN;
    END IF;
    
    -- Get parent's main account
    SELECT id, balance, version INTO v_parent_account_id, v_parent_balance, v_parent_version
    FROM accounts
    WHERE user_id = p_approver_id AND type = 0 AND is_active = true
    FOR UPDATE;
    
    IF NOT FOUND THEN
        RETURN QUERY SELECT false, 'Parent account not found'::TEXT, 0::DECIMAL(18,2);
        RETURN;
    END IF;
    
    -- Get kid's main account
    SELECT id, version INTO v_kid_account_id, v_kid_version
    FROM accounts
    WHERE user_id = v_request.kid_id AND type = 0 AND is_active = true
    FOR UPDATE;
    
    IF NOT FOUND THEN
        RETURN QUERY SELECT false, 'Kid account not found'::TEXT, 0::DECIMAL(18,2);
        RETURN;
    END IF;
    
    -- Check parent has enough funds
    IF v_parent_balance < v_request.amount THEN
        RETURN QUERY SELECT false, 'Insufficient funds'::TEXT, 0::DECIMAL(18,2);
        RETURN;
    END IF;
    
    -- Update request status
    UPDATE money_requests
    SET status = 1, -- Approved
        responded_by_id = p_approver_id,
        responded_at = NOW(),
        parent_note = p_note
    WHERE id = p_request_id;
    
    -- Debit parent
    UPDATE accounts 
    SET balance = balance - v_request.amount,
        version = version + 1,
        updated_at = NOW()
    WHERE id = v_parent_account_id AND version = v_parent_version;
    
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;
    IF v_rows_affected = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on parent account';
    END IF;
    
    -- Credit kid
    UPDATE accounts 
    SET balance = balance + v_request.amount,
        version = version + 1,
        updated_at = NOW()
    WHERE id = v_kid_account_id AND version = v_kid_version
    RETURNING balance INTO v_new_balance;
    
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;
    IF v_rows_affected = 0 THEN
        RAISE EXCEPTION 'Concurrency conflict on kid account';
    END IF;
    
    -- Create transaction records
    INSERT INTO transactions (id, account_id, type, amount, currency, balance_after, description, created_at)
    VALUES (
        gen_random_uuid(), v_parent_account_id, 1, -v_request.amount, v_request.currency,
        v_parent_balance - v_request.amount, 'Money request: ' || COALESCE(v_request.reason, 'No reason'), NOW()
    );
    
    INSERT INTO transactions (id, account_id, type, amount, currency, balance_after, description, created_at)
    VALUES (
        gen_random_uuid(), v_kid_account_id, 0, v_request.amount, v_request.currency,
        v_new_balance, 'Money request approved', NOW()
    );
    
    RETURN QUERY SELECT true, NULL::TEXT, v_new_balance;
END;
$$ LANGUAGE plpgsql;


-- 5. Calculate spending analytics
CREATE OR REPLACE FUNCTION sp_get_spending_analytics(
    p_kid_id UUID,
    p_from_date TIMESTAMP WITH TIME ZONE,
    p_to_date TIMESTAMP WITH TIME ZONE
)
RETURNS TABLE(
    total_spent DECIMAL(18,2),
    total_earned DECIMAL(18,2),
    total_saved DECIMAL(18,2),
    transaction_count INT,
    category_breakdown JSONB
) AS $$
DECLARE
    v_spent DECIMAL(18,2) := 0;
    v_earned DECIMAL(18,2) := 0;
    v_saved DECIMAL(18,2) := 0;
    v_count INT := 0;
    v_categories JSONB;
BEGIN
    -- Get kid's account IDs
    WITH kid_accounts AS (
        SELECT id FROM accounts WHERE user_id = p_kid_id AND is_active = true
    )
    SELECT 
        COALESCE(SUM(CASE WHEN t.amount < 0 THEN ABS(t.amount) ELSE 0 END), 0),
        COALESCE(SUM(CASE WHEN t.amount > 0 THEN t.amount ELSE 0 END), 0),
        COUNT(*)
    INTO v_spent, v_earned, v_count
    FROM transactions t
    JOIN kid_accounts ka ON t.account_id = ka.id
    WHERE t.created_at >= p_from_date AND t.created_at <= p_to_date;
    
    -- Calculate saved to goals
    SELECT COALESCE(SUM(saved_amount), 0)
    INTO v_saved
    FROM wishlist_goals
    WHERE user_id = p_kid_id 
      AND created_at >= p_from_date 
      AND created_at <= p_to_date;
    
    -- Get category breakdown (simplified - using description keywords)
    SELECT jsonb_object_agg(
        category,
        total
    )
    INTO v_categories
    FROM (
        SELECT 
            CASE 
                WHEN t.description ILIKE '%task%' THEN 'Tasks'
                WHEN t.description ILIKE '%goal%' THEN 'Goals'
                WHEN t.description ILIKE '%allowance%' THEN 'Allowance'
                WHEN t.description ILIKE '%request%' THEN 'Requests'
                ELSE 'Other'
            END as category,
            SUM(ABS(t.amount)) as total
        FROM transactions t
        JOIN accounts a ON t.account_id = a.id
        WHERE a.user_id = p_kid_id
          AND t.created_at >= p_from_date 
          AND t.created_at <= p_to_date
        GROUP BY 1
    ) sub;
    
    RETURN QUERY SELECT v_spent, v_earned, v_saved, v_count, COALESCE(v_categories, '{}'::jsonb);
END;
$$ LANGUAGE plpgsql;


-- 6. Process recurring allowances (to be called by a scheduled job)
CREATE OR REPLACE FUNCTION sp_process_recurring_allowances()
RETURNS TABLE(
    processed_count INT,
    total_amount DECIMAL(18,2),
    failed_count INT
) AS $$
DECLARE
    v_processed INT := 0;
    v_total DECIMAL(18,2) := 0;
    v_failed INT := 0;
    v_allowance RECORD;
    v_parent_account_id UUID;
    v_kid_account_id UUID;
    v_parent_balance DECIMAL(18,2);
BEGIN
    FOR v_allowance IN 
        SELECT * FROM recurring_allowances 
        WHERE is_active = true 
          AND next_payment_date <= NOW()
        FOR UPDATE
    LOOP
        BEGIN
            -- Get parent account
            SELECT id, balance INTO v_parent_account_id, v_parent_balance
            FROM accounts
            WHERE user_id = v_allowance.parent_id AND type = 0 AND is_active = true;
            
            -- Get kid account
            SELECT id INTO v_kid_account_id
            FROM accounts
            WHERE user_id = v_allowance.kid_id AND type = 0 AND is_active = true;
            
            -- Check if parent has funds
            IF v_parent_balance >= v_allowance.amount AND v_parent_account_id IS NOT NULL AND v_kid_account_id IS NOT NULL THEN
                -- Perform transfer
                PERFORM sp_transfer_between_accounts(
                    v_parent_account_id,
                    v_kid_account_id,
                    v_allowance.amount,
                    'Recurring allowance payment',
                    v_allowance.parent_id
                );
                
                -- Update allowance record
                UPDATE recurring_allowances
                SET last_payment_date = NOW(),
                    next_payment_date = CASE frequency
                        WHEN 'Weekly' THEN NOW() + INTERVAL '7 days'
                        WHEN 'BiWeekly' THEN NOW() + INTERVAL '14 days'
                        WHEN 'Monthly' THEN NOW() + INTERVAL '1 month'
                        ELSE NOW() + INTERVAL '7 days'
                    END
                WHERE id = v_allowance.id;
                
                v_processed := v_processed + 1;
                v_total := v_total + v_allowance.amount;
            ELSE
                v_failed := v_failed + 1;
            END IF;
        EXCEPTION WHEN OTHERS THEN
            v_failed := v_failed + 1;
        END;
    END LOOP;
    
    RETURN QUERY SELECT v_processed, v_total, v_failed;
END;
$$ LANGUAGE plpgsql;


-- 7. Validate spending against limits
CREATE OR REPLACE FUNCTION sp_validate_spending_limit(
    p_kid_id UUID,
    p_amount DECIMAL(18,2),
    p_category_id UUID DEFAULT NULL
)
RETURNS TABLE(
    is_allowed BOOLEAN,
    remaining_daily DECIMAL(18,2),
    remaining_weekly DECIMAL(18,2),
    remaining_monthly DECIMAL(18,2),
    reason TEXT
) AS $$
DECLARE
    v_daily_limit DECIMAL(18,2);
    v_weekly_limit DECIMAL(18,2);
    v_monthly_limit DECIMAL(18,2);
    v_spent_today DECIMAL(18,2);
    v_spent_week DECIMAL(18,2);
    v_spent_month DECIMAL(18,2);
    v_remaining_daily DECIMAL(18,2);
    v_remaining_weekly DECIMAL(18,2);
    v_remaining_monthly DECIMAL(18,2);
BEGIN
    -- Get limits
    SELECT 
        MAX(CASE WHEN period = 'Daily' THEN limit_amount END),
        MAX(CASE WHEN period = 'Weekly' THEN limit_amount END),
        MAX(CASE WHEN period = 'Monthly' THEN limit_amount END)
    INTO v_daily_limit, v_weekly_limit, v_monthly_limit
    FROM spending_limits
    WHERE kid_id = p_kid_id AND is_active = true;
    
    -- Calculate spent amounts
    WITH kid_accounts AS (
        SELECT id FROM accounts WHERE user_id = p_kid_id AND is_active = true
    )
    SELECT 
        COALESCE(SUM(CASE WHEN t.created_at >= DATE_TRUNC('day', NOW()) THEN ABS(t.amount) ELSE 0 END), 0),
        COALESCE(SUM(CASE WHEN t.created_at >= DATE_TRUNC('week', NOW()) THEN ABS(t.amount) ELSE 0 END), 0),
        COALESCE(SUM(CASE WHEN t.created_at >= DATE_TRUNC('month', NOW()) THEN ABS(t.amount) ELSE 0 END), 0)
    INTO v_spent_today, v_spent_week, v_spent_month
    FROM transactions t
    JOIN kid_accounts ka ON t.account_id = ka.id
    WHERE t.amount < 0;
    
    -- Calculate remaining
    v_remaining_daily := COALESCE(v_daily_limit - v_spent_today, 999999);
    v_remaining_weekly := COALESCE(v_weekly_limit - v_spent_week, 999999);
    v_remaining_monthly := COALESCE(v_monthly_limit - v_spent_month, 999999);
    
    -- Check limits
    IF v_daily_limit IS NOT NULL AND (v_spent_today + p_amount) > v_daily_limit THEN
        RETURN QUERY SELECT false, v_remaining_daily, v_remaining_weekly, v_remaining_monthly, 
            'Daily spending limit exceeded'::TEXT;
        RETURN;
    END IF;
    
    IF v_weekly_limit IS NOT NULL AND (v_spent_week + p_amount) > v_weekly_limit THEN
        RETURN QUERY SELECT false, v_remaining_daily, v_remaining_weekly, v_remaining_monthly,
            'Weekly spending limit exceeded'::TEXT;
        RETURN;
    END IF;
    
    IF v_monthly_limit IS NOT NULL AND (v_spent_month + p_amount) > v_monthly_limit THEN
        RETURN QUERY SELECT false, v_remaining_daily, v_remaining_weekly, v_remaining_monthly,
            'Monthly spending limit exceeded'::TEXT;
        RETURN;
    END IF;
    
    RETURN QUERY SELECT true, v_remaining_daily, v_remaining_weekly, v_remaining_monthly, NULL::TEXT;
END;
$$ LANGUAGE plpgsql;


-- Create indexes for performance
CREATE INDEX IF NOT EXISTS ix_transactions_account_date ON transactions(account_id, created_at);
CREATE INDEX IF NOT EXISTS ix_transactions_type ON transactions(type);
CREATE INDEX IF NOT EXISTS ix_accounts_user_type ON accounts(user_id, type);
CREATE INDEX IF NOT EXISTS ix_task_assignments_status ON task_assignments(status);
CREATE INDEX IF NOT EXISTS ix_money_requests_status ON money_requests(status);
CREATE INDEX IF NOT EXISTS ix_recurring_allowances_next_payment ON recurring_allowances(next_payment_date) WHERE is_active = true;

-- Grant execute permissions (adjust role names as needed)
-- GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO kidbank_app;
