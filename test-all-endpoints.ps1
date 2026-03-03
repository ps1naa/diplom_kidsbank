$baseUrl = "http://localhost:5000/api/v1"
$results = @()

function Test-Endpoint {
    param($Name, $Method, $Url, $Headers, $Body)
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            ContentType = "application/json"
        }
        if ($Headers) { $params.Headers = $Headers }
        if ($Body) { $params.Body = $Body }
        
        $response = Invoke-RestMethod @params
        Write-Host "[OK] $Name" -ForegroundColor Green
        return @{Name=$Name; Status="OK"; Response=$response}
    }
    catch {
        $status = $_.Exception.Response.StatusCode.value__
        Write-Host "[FAIL] $Name - Status: $status" -ForegroundColor Red
        return @{Name=$Name; Status="FAIL"; Error=$status}
    }
}

Write-Host "`n========== AUTH TESTS ==========" -ForegroundColor Cyan

# 1. Register Parent
$parentReg = Test-Endpoint "Register Parent" "POST" "$baseUrl/auth/register/parent" $null '{"email":"testparent@test.com","password":"Test123!","firstName":"Test","lastName":"Parent","dateOfBirth":"1985-01-01T00:00:00Z","familyName":"Test Family"}'
$results += $parentReg

# 2. Login Parent  
$parentLogin = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -ContentType "application/json" -Body '{"email":"parent@test.com","password":"Test123!"}'
$parentToken = $parentLogin.accessToken
$parentRefresh = $parentLogin.refreshToken
$parentHeaders = @{Authorization="Bearer $parentToken"}
Write-Host "[OK] Login Parent" -ForegroundColor Green
$results += @{Name="Login Parent"; Status="OK"}

# 3. Refresh Token
$refreshBody = '{"accessToken":"' + $parentToken + '","refreshToken":"' + $parentRefresh + '"}'
$results += Test-Endpoint "Refresh Token" "POST" "$baseUrl/auth/refresh" $null $refreshBody

# 4. Revoke All Tokens
$results += Test-Endpoint "Revoke All Tokens" "POST" "$baseUrl/auth/revoke-all" $parentHeaders '{}'

# Re-login after revoke
$parentLogin = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -ContentType "application/json" -Body '{"email":"parent@test.com","password":"Test123!"}'
$parentToken = $parentLogin.accessToken
$parentRefresh = $parentLogin.refreshToken
$parentHeaders = @{Authorization="Bearer $parentToken"}

# 5. Login Kid
$kidLogin = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -ContentType "application/json" -Body '{"email":"kid@test.com","password":"Kid123!"}'
$kidToken = $kidLogin.accessToken
$kidHeaders = @{Authorization="Bearer $kidToken"}
$kidId = $kidLogin.userId
Write-Host "[OK] Login Kid" -ForegroundColor Green
$results += @{Name="Login Kid"; Status="OK"}

Write-Host "`n========== USER TESTS ==========" -ForegroundColor Cyan

# 6. Get Current User (Parent)
$results += Test-Endpoint "Get Current User (Parent)" "GET" "$baseUrl/users/me" $parentHeaders $null

# 7. Get Current User (Kid)
$results += Test-Endpoint "Get Current User (Kid)" "GET" "$baseUrl/users/me" $kidHeaders $null

# 8. Update Profile
$results += Test-Endpoint "Update Profile" "PUT" "$baseUrl/users/me" $kidHeaders '{"firstName":"Updated","lastName":"Name"}'

# 9. Get User By Id
$results += Test-Endpoint "Get User By Id" "GET" "$baseUrl/users/$kidId" $parentHeaders $null

Write-Host "`n========== FAMILY TESTS ==========" -ForegroundColor Cyan

# 10. Get Family Dashboard
$results += Test-Endpoint "Get Family Dashboard" "GET" "$baseUrl/Families/dashboard" $parentHeaders $null

# 11. Get Kids
$results += Test-Endpoint "Get Kids" "GET" "$baseUrl/Families/kids" $parentHeaders $null

# 12. Generate Invitation
$inviteResult = Test-Endpoint "Generate Invitation" "POST" "$baseUrl/Families/invite" $parentHeaders '{}'
$results += $inviteResult

Write-Host "`n========== ACCOUNT TESTS ==========" -ForegroundColor Cyan

# 13. Get My Accounts (Kid)
$kidAccounts = Test-Endpoint "Get My Accounts (Kid)" "GET" "$baseUrl/accounts/my" $kidHeaders $null
$results += $kidAccounts
$kidAccountId = $kidAccounts.Response.value[0].id

# 14. Get My Accounts (Parent)
$parentAccounts = Test-Endpoint "Get My Accounts (Parent)" "GET" "$baseUrl/accounts/my" $parentHeaders $null
$results += $parentAccounts

# 15. Get Kid Accounts (by Parent)
$results += Test-Endpoint "Get Kid Accounts" "GET" "$baseUrl/accounts/kid/$kidId" $parentHeaders $null

# 16. Top Up (Parent)
$results += Test-Endpoint "Top Up Account" "POST" "$baseUrl/accounts/topup" $parentHeaders '{"amount":1000}'

# 17. Deposit to Kid
$depositBody = '{"kidId":"' + $kidId + '","amount":100,"description":"Test deposit"}'
$results += Test-Endpoint "Deposit to Kid" "POST" "$baseUrl/accounts/deposit" $parentHeaders $depositBody

# 18. Create Savings Account
$results += Test-Endpoint "Create Savings Account" "POST" "$baseUrl/accounts/savings" $kidHeaders '{"name":"My Savings"}'

# 19. Get Transactions
$results += Test-Endpoint "Get Transactions" "GET" "$baseUrl/accounts/$kidAccountId/transactions" $kidHeaders $null

# 20. Transfer Between Accounts (need savings account first)
$kidAccounts2 = Invoke-RestMethod -Uri "$baseUrl/accounts/my" -Method GET -Headers $kidHeaders
if ($kidAccounts2.value.Count -gt 1) {
    $savingsId = ($kidAccounts2.value | Where-Object { $_.type -eq "Savings" })[0].id
    $transferBody = '{"sourceAccountId":"' + $kidAccountId + '","destinationAccountId":"' + $savingsId + '","amount":10}'
    $results += Test-Endpoint "Transfer Between Accounts" "POST" "$baseUrl/accounts/transfer" $kidHeaders $transferBody
}

Write-Host "`n========== GOAL TESTS ==========" -ForegroundColor Cyan

# 21. Create Goal
$goalResult = Test-Endpoint "Create Goal" "POST" "$baseUrl/goals" $kidHeaders '{"title":"Test Goal","targetAmount":1000,"description":"Test"}'
$results += $goalResult
$goalId = $goalResult.Response.id

# 22. Get My Goals
$results += Test-Endpoint "Get My Goals" "GET" "$baseUrl/goals/my" $kidHeaders $null

# 23. Get Kid Goals (Parent)
$results += Test-Endpoint "Get Kid Goals" "GET" "$baseUrl/goals/kid/$kidId" $parentHeaders $null

# 24. Update Goal
$results += Test-Endpoint "Update Goal" "PUT" "$baseUrl/goals/$goalId" $kidHeaders '{"title":"Updated Goal","targetAmount":2000}'

# 25. Deposit to Goal
$depositGoalBody = '{"amount":50}'
$results += Test-Endpoint "Deposit to Goal" "POST" "$baseUrl/goals/$goalId/deposit" $kidHeaders $depositGoalBody

Write-Host "`n========== TASK TESTS ==========" -ForegroundColor Cyan

# 26. Create Task
$taskBody = '{"assignedToId":"' + $kidId + '","title":"Test Task","description":"Do something","rewardAmount":25}'
$taskResult = Test-Endpoint "Create Task" "POST" "$baseUrl/tasks" $parentHeaders $taskBody
$results += $taskResult
$taskId = $taskResult.Response.id

# 27. Get Assigned Tasks (Kid)
$results += Test-Endpoint "Get Assigned Tasks" "GET" "$baseUrl/tasks/my" $kidHeaders $null

# 28. Get Pending Approval Tasks (Parent)
$results += Test-Endpoint "Get Pending Approval Tasks" "GET" "$baseUrl/tasks/pending-approval" $parentHeaders $null

# 29. Update Task
$results += Test-Endpoint "Update Task" "PUT" "$baseUrl/tasks/$taskId" $parentHeaders '{"title":"Updated Task","rewardAmount":30}'

# 30. Complete Task
$results += Test-Endpoint "Complete Task" "POST" "$baseUrl/tasks/$taskId/complete" $kidHeaders '{}'

# 31. Create another task for reject test
$task2Body = '{"assignedToId":"' + $kidId + '","title":"Task to Reject","rewardAmount":10}'
$task2Result = Test-Endpoint "Create Task 2" "POST" "$baseUrl/tasks" $parentHeaders $task2Body
$results += $task2Result
$task2Id = $task2Result.Response.id

# 32. Complete Task 2
$results += Test-Endpoint "Complete Task 2" "POST" "$baseUrl/tasks/$task2Id/complete" $kidHeaders '{}'

# 33. Reject Task
$results += Test-Endpoint "Reject Task" "POST" "$baseUrl/tasks/$task2Id/reject" $parentHeaders '{"reason":"Not done properly"}'

# 34. Approve Task
$results += Test-Endpoint "Approve Task" "POST" "$baseUrl/tasks/$taskId/approve" $parentHeaders '{}'

Write-Host "`n========== MONEY REQUEST TESTS ==========" -ForegroundColor Cyan

# 35. Create Money Request
$moneyReqResult = Test-Endpoint "Create Money Request" "POST" "$baseUrl/MoneyRequests" $kidHeaders '{"amount":50,"reason":"Need money"}'
$results += $moneyReqResult
$moneyReqId = $moneyReqResult.Response.id

# 36. Get My Money Requests (Kid)
$results += Test-Endpoint "Get My Money Requests" "GET" "$baseUrl/MoneyRequests/my" $kidHeaders $null

# 37. Get Pending Money Requests (Parent)
$results += Test-Endpoint "Get Pending Money Requests" "GET" "$baseUrl/MoneyRequests/pending" $parentHeaders $null

# 38. Create another request to reject
$moneyReq2Result = Test-Endpoint "Create Money Request 2" "POST" "$baseUrl/MoneyRequests" $kidHeaders '{"amount":100,"reason":"Another request"}'
$results += $moneyReq2Result
$moneyReq2Id = $moneyReq2Result.Response.id

# 39. Reject Money Request
$results += Test-Endpoint "Reject Money Request" "POST" "$baseUrl/MoneyRequests/$moneyReq2Id/reject" $parentHeaders '{"note":"Too much"}'

# 40. Approve Money Request
$results += Test-Endpoint "Approve Money Request" "POST" "$baseUrl/MoneyRequests/$moneyReqId/approve" $parentHeaders '{}'

Write-Host "`n========== SPENDING LIMIT TESTS ==========" -ForegroundColor Cyan

# 41. Set Spending Limit
$limitBody = '{"kidId":"' + $kidId + '","limitAmount":200,"period":"Daily"}'
$limitResult = Test-Endpoint "Set Spending Limit" "POST" "$baseUrl/SpendingLimits" $parentHeaders $limitBody
$results += $limitResult
$limitId = $limitResult.Response.id

# 42. Get Spending Limits
$results += Test-Endpoint "Get Spending Limits" "GET" "$baseUrl/SpendingLimits/kid/$kidId" $parentHeaders $null

# 43. Update Spending Limit
$results += Test-Endpoint "Update Spending Limit" "PUT" "$baseUrl/SpendingLimits/$limitId" $parentHeaders '{"limitAmount":300}'

Write-Host "`n========== ANALYTICS TESTS ==========" -ForegroundColor Cyan

# 44. Get Kid Spending Summary
$results += Test-Endpoint "Get Kid Spending Summary" "GET" "$baseUrl/Analytics/kid/$kidId/summary" $parentHeaders $null

# 45. Get Monthly Stats
$results += Test-Endpoint "Get Monthly Stats" "GET" "$baseUrl/Analytics/kid/$kidId/monthly" $parentHeaders $null

# 46. Get Family Analytics
$results += Test-Endpoint "Get Family Analytics" "GET" "$baseUrl/Analytics/family/overview" $parentHeaders $null

Write-Host "`n========== EDUCATION TESTS ==========" -ForegroundColor Cyan

# 47. Get Lessons
$results += Test-Endpoint "Get Lessons" "GET" "$baseUrl/Education/lessons" $kidHeaders $null

# 48. Get Education Progress
$results += Test-Endpoint "Get Education Progress" "GET" "$baseUrl/Education/progress" $kidHeaders $null

# 49. Update Streak
$results += Test-Endpoint "Update Streak" "POST" "$baseUrl/Education/streak/update" $kidHeaders '{}'

# 50. Add XP
$xpBody = '{"userId":"' + $kidId + '","amount":50}'
$results += Test-Endpoint "Add XP" "POST" "$baseUrl/Education/xp/add" $kidHeaders $xpBody

Write-Host "`n========== CHAT TESTS ==========" -ForegroundColor Cyan

# 51. Get Chat History
$results += Test-Endpoint "Get Chat History" "GET" "$baseUrl/Chat/history" $kidHeaders $null

Write-Host "`n========== LOGOUT TEST ==========" -ForegroundColor Cyan

# 52. Logout
$logoutBody = '{"refreshToken":"' + $parentRefresh + '"}'
$results += Test-Endpoint "Logout" "POST" "$baseUrl/auth/logout" $parentHeaders $logoutBody

Write-Host "`n========== SUMMARY ==========" -ForegroundColor Cyan
$passed = ($results | Where-Object { $_.Status -eq "OK" }).Count
$failed = ($results | Where-Object { $_.Status -eq "FAIL" }).Count
Write-Host "Total: $($results.Count) | Passed: $passed | Failed: $failed" -ForegroundColor Yellow

if ($failed -gt 0) {
    Write-Host "`nFailed tests:" -ForegroundColor Red
    $results | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object { Write-Host "  - $($_.Name): $($_.Error)" -ForegroundColor Red }
}
