function Write-PaketTemplateFiles()
{
    $relNotes = Get-Content (Join-Path $PSScriptRoot "releasenotes.md")
    Get-ChildItem $PSScriptRoot -Recurse -Filter *.tmpl | % {
        $paketFile = Get-Content $_.FullName
        $paketFile += "`nreleaseNotes`n"
        $relNotes | % {
            $paketFile += "`t$_`n"
        } 
        $newFile = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName($_.FullName),"paket.template")
        Set-Content -Value $paketFile -Path $newFile
    }    
}

Write-PaketTemplateFiles