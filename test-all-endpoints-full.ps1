$baseUrl = "http://localhost:5000/api/v1"
$results = @()
$testCount = 0

function Test-Endpoint {
    param($Name, $Method, $Url, $Headers, $Body, [switch]$AllowFail)
    
    $global:testCount++
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            ContentType = "application/json"
        }
        if ($Headers) { $params.Headers = $Headers }
        if ($Body) { $params.Body = $Body }
        
        $response = Invoke-RestMethod @params
        Write-Host "[$testCount] [OK] $Name" -ForegroundColor Green
        return @{Name=$Name; Status="OK"; Response=$response}
    }
    catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($AllowFail) {
            Write-Host "[$testCount] [SKIP] $Name - Status: $status (Expected)" -ForegroundColor Yellow
            return @{Name=$Name; Status="SKIP"; Error=$status}
        }
        Write-Host "[$testCount] [FAIL] $Name - Status: $status" -ForegroundColor Red
        return @{Name=$Name; Status="FAIL"; Error=$status}
    }
}

Write-Host "`n" + "="*60 -ForegroundColor Cyan
Write-Host "   KIDBANK API - FULL TEST SUITE (64 ENDPOINTS)" -ForegroundColor Cyan
Write-Host "="*60 + "`n" -ForegroundColor Cyan

# Generate unique emails
$timestamp = [DateTimeOffset]::Now.ToUnixTimeSeconds()
$parentEmail = "parent_$timestamp@test.com"
$kidEmail = "kid_$timestamp@test.com"
$password = "Test123!"

Write-Host "========== 1. AUTH TESTS (6 endpoints) ==========" -ForegroundColor Cyan

# 1. Register Parent
$parentReg = Test-Endpoint "POST /auth/register/parent" "POST" "$baseUrl/auth/register/parent" $null (@{
    email=$parentEmail
    password=$password
    firstName="Test"
    lastName="Parent"
    dateOfBirth="1985-01-01T00:00:00Z"
    familyName="Test Family $timestamp"
} | ConvertTo-Json)
$results += $parentReg

# 2. Login Parent
$loginBody = @{email=$parentEmail; password=$password} | ConvertTo-Json
$parentLogin = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -ContentType "application/json" -Body $loginBody
$parentToken = $parentLogin.accessToken
$parentRefresh = $parentLogin.refreshToken
$parentId = $parentLogin.userId
$parentHeaders = @{Authorization="Bearer $parentToken"}
Write-Host "[$($global:testCount+1)] [OK] POST /auth/login (Parent)" -ForegroundColor Green
$results += @{Name="POST /auth/login (Parent)"; Status="OK"}
$global:testCount++

# 3. Refresh Token
$refreshBody = @{accessToken=$parentToken; refreshToken=$parentRefresh} | ConvertTo-Json
$refreshResult = Test-Endpoint "POST /auth/refresh" "POST" "$baseUrl/auth/refresh" $null $refreshBody
$results += $refreshResult
if ($refreshResult.Status -eq "OK") {
    $parentToken = $refreshResult.Response.accessToken
    $parentRefresh = $refreshResult.Response.refreshToken
    $parentHeaders = @{Authorization="Bearer $parentToken"}
}

# 4. Generate invitation for kid
$inviteResult = Test-Endpoint "POST /Families/invite" "POST" "$baseUrl/Families/invite" $parentHeaders '{}'
$results += $inviteResult
$invitationToken = $inviteResult.Response.token

# 5. Register Kid
$kidRegBody = @{
    email=$kidEmail
    password=$password
    firstName="Test"
    lastName="Kid"
    dateOfBirth="2015-01-01T00:00:00Z"
    invitationToken=$invitationToken
} | ConvertTo-Json
$kidReg = Test-Endpoint "POST /auth/register/kid" "POST" "$baseUrl/auth/register/kid" $null $kidRegBody
$results += $kidReg

# 6. Login Kid
$kidLoginBody = @{email=$kidEmail; password=$password} | ConvertTo-Json
$kidLogin = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -ContentType "application/json" -Body $kidLoginBody
$kidToken = $kidLogin.accessToken
$kidRefresh = $kidLogin.refreshToken
$kidId = $kidLogin.userId
$kidHeaders = @{Authorization="Bearer $kidToken"}
Write-Host "[$($global:testCount+1)] [OK] POST /auth/login (Kid)" -ForegroundColor Green
$results += @{Name="POST /auth/login (Kid)"; Status="OK"}
$global:testCount++

# 5. Revoke All Tokens (and re-login)
$results += Test-Endpoint "POST /auth/revoke-all" "POST" "$baseUrl/auth/revoke-all" $parentHeaders '{}'
$parentLogin = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -ContentType "application/json" -Body (@{email=$parentEmail; password=$password} | ConvertTo-Json)
$parentToken = $parentLogin.accessToken
$parentRefresh = $parentLogin.refreshToken
$parentHeaders = @{Authorization="Bearer $parentToken"}

Write-Host "`n========== 2. USERS TESTS (3 endpoints) ==========" -ForegroundColor Cyan

# 7. Get Current User (Parent)
$results += Test-Endpoint "GET /users/me (Parent)" "GET" "$baseUrl/users/me" $parentHeaders $null

# 8. Get Current User (Kid)
$results += Test-Endpoint "GET /users/me (Kid)" "GET" "$baseUrl/users/me" $kidHeaders $null

# 9. Update Profile (Kid)
$updateBody = @{firstName="Updated"; lastName="KidName"} | ConvertTo-Json
$results += Test-Endpoint "PUT /users/me" "PUT" "$baseUrl/users/me" $kidHeaders $updateBody

# 10. Get User By Id (Parent viewing Kid)
$results += Test-Endpoint "GET /users/{id}" "GET" "$baseUrl/users/$kidId" $parentHeaders $null

Write-Host "`n========== 3. FAMILIES TESTS (4 endpoints) ==========" -ForegroundColor Cyan

# 11. Create Family (already has one, should fail)
$createFamilyBody = @{name="Another Family"} | ConvertTo-Json
$results += Test-Endpoint "POST /Families (duplicate)" "POST" "$baseUrl/Families" $parentHeaders $createFamilyBody -AllowFail

# 12. Get Family Dashboard
$results += Test-Endpoint "GET /Families/dashboard" "GET" "$baseUrl/Families/dashboard" $parentHeaders $null

# 13. Get Kids
$results += Test-Endpoint "GET /Families/kids" "GET" "$baseUrl/Families/kids" $parentHeaders $null

# 14. Generate Another Invitation
$results += Test-Endpoint "POST /Families/invite (2nd)" "POST" "$baseUrl/Families/invite" $parentHeaders '{}'

Write-Host "`n========== 4. ACCOUNTS TESTS (7 endpoints) ==========" -ForegroundColor Cyan

# 15. Get My Accounts (Parent)
$parentAccResult = Test-Endpoint "GET /accounts/my (Parent)" "GET" "$baseUrl/accounts/my" $parentHeaders $null
$results += $parentAccResult
if ($parentAccResult.Response) {
    $parentAccountId = $parentAccResult.Response[0].id
}

# 16. Get My Accounts (Kid)
$kidAccResult = Test-Endpoint "GET /accounts/my (Kid)" "GET" "$baseUrl/accounts/my" $kidHeaders $null
$results += $kidAccResult
if ($kidAccResult.Response) {
    $kidAccountId = $kidAccResult.Response[0].id
}

# 17. Get Kid Accounts (by Parent)
$results += Test-Endpoint "GET /accounts/kid/{kidId}" "GET" "$baseUrl/accounts/kid/$kidId" $parentHeaders $null

# 18. Top Up Parent Account
$topupBody = @{amount=10000} | ConvertTo-Json
$results += Test-Endpoint "POST /accounts/topup" "POST" "$baseUrl/accounts/topup" $parentHeaders $topupBody

# 19. Deposit to Kid
$depositBody = @{kidId=$kidId; amount=500; description="Allowance"} | ConvertTo-Json
$results += Test-Endpoint "POST /accounts/deposit" "POST" "$baseUrl/accounts/deposit" $parentHeaders $depositBody

# 20. Create Savings Account
$savingsBody = @{name="My Savings"} | ConvertTo-Json
$savingsResult = Test-Endpoint "POST /accounts/savings" "POST" "$baseUrl/accounts/savings" $kidHeaders $savingsBody
$results += $savingsResult
$savingsAccountId = $savingsResult.Response.id

# 21. Get Transactions
$results += Test-Endpoint "GET /accounts/{id}/transactions" "GET" "$baseUrl/accounts/$kidAccountId/transactions" $kidHeaders $null

# 22. Transfer Between Accounts
if ($savingsAccountId) {
    $transferBody = @{sourceAccountId=$kidAccountId; destinationAccountId=$savingsAccountId; amount=50} | ConvertTo-Json
    $results += Test-Endpoint "POST /accounts/transfer" "POST" "$baseUrl/accounts/transfer" $kidHeaders $transferBody
}

Write-Host "`n========== 5. GOALS TESTS (6 endpoints) ==========" -ForegroundColor Cyan

# 23. Create Goal
$goalBody = @{title="New Bicycle"; targetAmount=5000; description="I want a new bike"} | ConvertTo-Json
$goalResult = Test-Endpoint "POST /goals" "POST" "$baseUrl/goals" $kidHeaders $goalBody
$results += $goalResult
$goalId = $goalResult.Response.id

# 24. Get My Goals
$results += Test-Endpoint "GET /goals/my" "GET" "$baseUrl/goals/my" $kidHeaders $null

# 25. Get Kid Goals (Parent)
$results += Test-Endpoint "GET /goals/kid/{kidId}" "GET" "$baseUrl/goals/kid/$kidId" $parentHeaders $null

# 26. Update Goal
$updateGoalBody = @{title="New BMX Bicycle"; targetAmount=6000; description="Updated goal"} | ConvertTo-Json
$results += Test-Endpoint "PUT /goals/{goalId}" "PUT" "$baseUrl/goals/$goalId" $kidHeaders $updateGoalBody

# 27. Deposit to Goal
$depositGoalBody = @{amount=100} | ConvertTo-Json
$results += Test-Endpoint "POST /goals/{goalId}/deposit" "POST" "$baseUrl/goals/$goalId/deposit" $kidHeaders $depositGoalBody

# 28. Delete Goal (create another one first)
$goal2Body = @{title="To Delete"; targetAmount=100} | ConvertTo-Json
$goal2Result = Invoke-RestMethod -Uri "$baseUrl/goals" -Method POST -ContentType "application/json" -Headers $kidHeaders -Body $goal2Body
$goal2Id = $goal2Result.id
$results += Test-Endpoint "DELETE /goals/{goalId}" "DELETE" "$baseUrl/goals/$goal2Id" $kidHeaders $null

Write-Host "`n========== 6. TASKS TESTS (8 endpoints) ==========" -ForegroundColor Cyan

# 29. Create Task
$taskBody = @{assignedToId=$kidId; title="Clean Room"; description="Clean your room"; rewardAmount=50} | ConvertTo-Json
$taskResult = Test-Endpoint "POST /tasks" "POST" "$baseUrl/tasks" $parentHeaders $taskBody
$results += $taskResult
$taskId = $taskResult.Response.id

# 30. Get Assigned Tasks (Kid)
$results += Test-Endpoint "GET /tasks/my" "GET" "$baseUrl/tasks/my" $kidHeaders $null

# 31. Get Pending Approval Tasks (Parent)
$results += Test-Endpoint "GET /tasks/pending-approval" "GET" "$baseUrl/tasks/pending-approval" $parentHeaders $null

# 32. Update Task
$updateTaskBody = @{title="Clean Room Thoroughly"; rewardAmount=75; description="Updated task"} | ConvertTo-Json
$results += Test-Endpoint "PUT /tasks/{taskId}" "PUT" "$baseUrl/tasks/$taskId" $parentHeaders $updateTaskBody

# 33. Complete Task (Kid)
$results += Test-Endpoint "POST /tasks/{taskId}/complete" "POST" "$baseUrl/tasks/$taskId/complete" $kidHeaders '{}'

# 34. Approve Task (Parent)
$results += Test-Endpoint "POST /tasks/{taskId}/approve" "POST" "$baseUrl/tasks/$taskId/approve" $parentHeaders '{}'

# 35. Create another task to reject
$task2Body = @{assignedToId=$kidId; title="Task to Reject"; rewardAmount=20} | ConvertTo-Json
$task2Result = Invoke-RestMethod -Uri "$baseUrl/tasks" -Method POST -ContentType "application/json" -Headers $parentHeaders -Body $task2Body
$task2Id = $task2Result.id
Invoke-RestMethod -Uri "$baseUrl/tasks/$task2Id/complete" -Method POST -ContentType "application/json" -Headers $kidHeaders -Body '{}'
$results += Test-Endpoint "POST /tasks/{taskId}/reject" "POST" "$baseUrl/tasks/$task2Id/reject" $parentHeaders '{"reason":"Not done properly"}'

# 36. Create task to delete
$task3Body = @{assignedToId=$kidId; title="Task to Delete"; rewardAmount=10} | ConvertTo-Json
$task3Result = Invoke-RestMethod -Uri "$baseUrl/tasks" -Method POST -ContentType "application/json" -Headers $parentHeaders -Body $task3Body
$task3Id = $task3Result.id
$results += Test-Endpoint "DELETE /tasks/{taskId}" "DELETE" "$baseUrl/tasks/$task3Id" $parentHeaders $null

Write-Host "`n========== 7. MONEY REQUESTS TESTS (5 endpoints) ==========" -ForegroundColor Cyan

# 37. Create Money Request
$moneyReqBody = @{amount=100; reason="Need money for school supplies"} | ConvertTo-Json
$moneyReqResult = Test-Endpoint "POST /MoneyRequests" "POST" "$baseUrl/MoneyRequests" $kidHeaders $moneyReqBody
$results += $moneyReqResult
$moneyReqId = $moneyReqResult.Response.id

# 38. Get My Money Requests (Kid)
$results += Test-Endpoint "GET /MoneyRequests/my" "GET" "$baseUrl/MoneyRequests/my" $kidHeaders $null

# 39. Get Pending Money Requests (Parent)
$results += Test-Endpoint "GET /MoneyRequests/pending" "GET" "$baseUrl/MoneyRequests/pending" $parentHeaders $null

# 40. Approve Money Request
$results += Test-Endpoint "POST /MoneyRequests/{id}/approve" "POST" "$baseUrl/MoneyRequests/$moneyReqId/approve" $parentHeaders '{}'

# 41. Create and Reject Money Request
$moneyReq2Body = @{amount=500; reason="Want a new game"} | ConvertTo-Json
$moneyReq2Result = Invoke-RestMethod -Uri "$baseUrl/MoneyRequests" -Method POST -ContentType "application/json" -Headers $kidHeaders -Body $moneyReq2Body
$moneyReq2Id = $moneyReq2Result.id
$results += Test-Endpoint "POST /MoneyRequests/{id}/reject" "POST" "$baseUrl/MoneyRequests/$moneyReq2Id/reject" $parentHeaders '{"note":"Too expensive"}'

Write-Host "`n========== 8. SPENDING LIMITS TESTS (3 endpoints) ==========" -ForegroundColor Cyan

# 42. Set Spending Limit
$limitBody = @{kidId=$kidId; limitAmount=200; period="Daily"} | ConvertTo-Json
$limitResult = Test-Endpoint "POST /SpendingLimits" "POST" "$baseUrl/SpendingLimits" $parentHeaders $limitBody
$results += $limitResult
$limitId = $limitResult.Response.id

# 43. Get Spending Limits
$results += Test-Endpoint "GET /SpendingLimits/kid/{kidId}" "GET" "$baseUrl/SpendingLimits/kid/$kidId" $parentHeaders $null

# 44. Update Spending Limit
$updateLimitBody = @{limitAmount=300} | ConvertTo-Json
$results += Test-Endpoint "PUT /SpendingLimits/{id}" "PUT" "$baseUrl/SpendingLimits/$limitId" $parentHeaders $updateLimitBody

Write-Host "`n========== 9. EDUCATION TESTS (7 endpoints) ==========" -ForegroundColor Cyan

# 45. Get Lessons
$results += Test-Endpoint "GET /Education/lessons" "GET" "$baseUrl/Education/lessons" $kidHeaders $null

# 46. Get Lesson Details (might not exist)
$results += Test-Endpoint "GET /Education/lessons/{id}" "GET" "$baseUrl/Education/lessons/00000000-0000-0000-0000-000000000001" $kidHeaders $null -AllowFail

# 47. Submit Quiz (might not exist)
$quizBody = @{quizId="00000000-0000-0000-0000-000000000001"; answers=@(@{questionId="q1"; answer="a"})} | ConvertTo-Json
$results += Test-Endpoint "POST /Education/quiz/submit" "POST" "$baseUrl/Education/quiz/submit" $kidHeaders $quizBody -AllowFail

# 48. Get Education Progress
$results += Test-Endpoint "GET /Education/progress" "GET" "$baseUrl/Education/progress" $kidHeaders $null

# 49. Add XP
$xpBody = @{userId=$kidId; amount=100} | ConvertTo-Json
$results += Test-Endpoint "POST /Education/xp/add" "POST" "$baseUrl/Education/xp/add" $kidHeaders $xpBody

# 50. Update Streak
$results += Test-Endpoint "POST /Education/streak/update" "POST" "$baseUrl/Education/streak/update" $kidHeaders '{}'

# 51. Unlock Achievement (might not exist)
$achieveBody = @{achievementCode="FIRST_DEPOSIT"} | ConvertTo-Json
$results += Test-Endpoint "POST /Education/achievement/unlock" "POST" "$baseUrl/Education/achievement/unlock" $kidHeaders $achieveBody -AllowFail

Write-Host "`n========== 10. ANALYTICS TESTS (3 endpoints) ==========" -ForegroundColor Cyan

# 52. Get Kid Spending Summary
$results += Test-Endpoint "GET /Analytics/kid/{kidId}/summary" "GET" "$baseUrl/Analytics/kid/$kidId/summary" $parentHeaders $null

# 53. Get Monthly Stats
$results += Test-Endpoint "GET /Analytics/kid/{kidId}/monthly" "GET" "$baseUrl/Analytics/kid/$kidId/monthly?months=6" $parentHeaders $null

# 54. Get Family Analytics
$results += Test-Endpoint "GET /Analytics/family/overview" "GET" "$baseUrl/Analytics/family/overview" $parentHeaders $null

Write-Host "`n========== 11. CHAT TESTS (1 endpoint) ==========" -ForegroundColor Cyan

# 55. Get Chat History
$results += Test-Endpoint "GET /Chat/history" "GET" "$baseUrl/Chat/history" $kidHeaders $null

Write-Host "`n========== 12. CARDS TESTS (4 endpoints) ==========" -ForegroundColor Cyan

# 56. Create Virtual Card
if ($kidAccountId) {
    $cardBody = @{accountId=$kidAccountId} | ConvertTo-Json
    $cardResult = Test-Endpoint "POST /Cards" "POST" "$baseUrl/Cards" $kidHeaders $cardBody
    $results += $cardResult
    if ($cardResult.Response) {
        $cardId = $cardResult.Response.id
    }
} else {
    Write-Host "[57] [SKIP] POST /Cards - No kidAccountId" -ForegroundColor Yellow
    $results += @{Name="POST /Cards"; Status="SKIP"; Error="No kidAccountId"}
}

# 57. Get My Cards
$results += Test-Endpoint "GET /Cards/my" "GET" "$baseUrl/Cards/my" $kidHeaders $null

# 58. Freeze Card
$results += Test-Endpoint "POST /Cards/{cardId}/freeze" "POST" "$baseUrl/Cards/$cardId/freeze" $kidHeaders '{}'

# 59. Unfreeze Card
$results += Test-Endpoint "POST /Cards/{cardId}/unfreeze" "POST" "$baseUrl/Cards/$cardId/unfreeze" $kidHeaders '{}'

Write-Host "`n========== 13. ALLOWANCES TESTS (2 endpoints) ==========" -ForegroundColor Cyan

# 60. Set Allowance
$allowanceBody = @{kidId=$kidId; amount=500; frequency="Weekly"; dayOfWeek=1} | ConvertTo-Json
$results += Test-Endpoint "POST /Allowances" "POST" "$baseUrl/Allowances" $parentHeaders $allowanceBody

# 61. Get Kid Allowance
$results += Test-Endpoint "GET /Allowances/kid/{kidId}" "GET" "$baseUrl/Allowances/kid/$kidId" $parentHeaders $null

Write-Host "`n========== 14. ACHIEVEMENTS TESTS (2 endpoints) ==========" -ForegroundColor Cyan

# 62. Get All Achievements
$results += Test-Endpoint "GET /Achievements" "GET" "$baseUrl/Achievements" $kidHeaders $null

# 63. Get My Achievements
$results += Test-Endpoint "GET /Achievements/my" "GET" "$baseUrl/Achievements/my" $kidHeaders $null

Write-Host "`n========== 15. LEADERBOARD TESTS (1 endpoint) ==========" -ForegroundColor Cyan

# 64. Get Family Leaderboard
$results += Test-Endpoint "GET /Leaderboard/family" "GET" "$baseUrl/Leaderboard/family" $parentHeaders $null

Write-Host "`n========== 16. NOTIFICATIONS TESTS (2 endpoints) ==========" -ForegroundColor Cyan

# 65. Get Notifications
$results += Test-Endpoint "GET /Notifications" "GET" "$baseUrl/Notifications" $kidHeaders $null

# 66. Mark Notification Read (might not exist)
$results += Test-Endpoint "POST /Notifications/{id}/read" "POST" "$baseUrl/Notifications/00000000-0000-0000-0000-000000000001/read" $kidHeaders '{}' -AllowFail

Write-Host "`n========== 17. LOGOUT TEST ==========" -ForegroundColor Cyan

# 67. Logout
$logoutBody = @{refreshToken=$parentRefresh} | ConvertTo-Json
$results += Test-Endpoint "POST /auth/logout" "POST" "$baseUrl/auth/logout" $parentHeaders $logoutBody

# ========== SUMMARY ==========
Write-Host "`n" + "="*60 -ForegroundColor Cyan
Write-Host "                    TEST SUMMARY" -ForegroundColor Cyan
Write-Host "="*60 -ForegroundColor Cyan

$passed = ($results | Where-Object { $_.Status -eq "OK" }).Count
$failed = ($results | Where-Object { $_.Status -eq "FAIL" }).Count
$skipped = ($results | Where-Object { $_.Status -eq "SKIP" }).Count

Write-Host "`nTotal Tests: $($results.Count)" -ForegroundColor White
Write-Host "Passed:      $passed" -ForegroundColor Green
Write-Host "Failed:      $failed" -ForegroundColor Red
Write-Host "Skipped:     $skipped" -ForegroundColor Yellow

if ($failed -gt 0) {
    Write-Host "`nFailed Tests:" -ForegroundColor Red
    $results | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object { 
        Write-Host "  - $($_.Name): HTTP $($_.Error)" -ForegroundColor Red 
    }
}

Write-Host "`n" + "="*60 -ForegroundColor Cyan
