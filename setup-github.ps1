# GitHub Repository Setup Script
# Usage: .\setup-github.ps1 -RepoUrl "https://github.com/yourusername/TradingPlatform.git"

param(
    [Parameter(Mandatory=$true)]
    [string]$RepoUrl
)

Write-Host "Setting up GitHub remote..." -ForegroundColor Cyan

# Add the remote
git remote add origin $RepoUrl

# Verify the remote was added
Write-Host "`nRemote configured:" -ForegroundColor Green
git remote -v

# Rename branch to main (GitHub's default)
Write-Host "`nRenaming branch to 'main'..." -ForegroundColor Cyan
git branch -M main

# Push to GitHub
Write-Host "`nPushing to GitHub..." -ForegroundColor Cyan
git push -u origin main

Write-Host "`n✅ Successfully pushed to GitHub!" -ForegroundColor Green
Write-Host "Repository URL: $RepoUrl" -ForegroundColor Yellow
