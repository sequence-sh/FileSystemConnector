[CmdletBinding()]
Param (
    # New connector name
    [Parameter(Mandatory=$true,
               Position=1)]
    [string]$Name,

    # New namespace for the project. Default is ''.
    [Parameter(Mandatory=$false,
               Position=2)]
    [string]$Namespace = 'Reductech.EDR.Connectors',

    # The url of the new project. By default this is generated from the
    # `git remote get-url origin` command.
    [Parameter(Mandatory=$false)]
    [string]$NewUrl,

    # The template name. Default is' EDRConnector'.
    [Parameter(Mandatory=$false)]
    [string]$TemplateName = 'EDRConnector',

    # The template namespace. Default is 'Reductech.EDR.Connectors.Template'.
    [Parameter(Mandatory=$false)]
    [string]$TemplateNamespace = 'Reductech.Templates',

    # The template url. Default is 'https://gitlab.com/reductech/templates/edr-connector'.
    [Parameter(Mandatory=$false)]
    [string]$TemplateUrl = 'https://gitlab.com/reductech/templates/edr-connector',

    # Do not remove content from the readme.
    [Parameter(Mandatory=$false)]
    [switch]$SkipReadme
)

if (!$NewUrl) {
    $gitUrl = git remote get-url origin
    $NewUrl = $gitUrl -replace '\.git$' -replace 'git@gitlab\.com:', 'https://gitlab.com/'
}

$encoding = [System.Text.UTF8Encoding]::new($false)

$templateFullName = $TemplateNamespace + '.' + $TemplateName
$fullName = $Namespace + '.' + $Name

@(
    "$TemplateName/$TemplateName.csproj"
    "$TemplateName/CheckFileExists.cs"
    "$TemplateName.Tests/$TemplateName.Tests.csproj"
    "$TemplateName.Tests/CheckFileExistsTests.cs"
) | ForEach-Object {
    $path = Join-Path (Get-Location).Path $_
    $content = Get-Content $path -Raw
    $newContent = $content -replace [regex]::Escape($TemplateUrl), $NewUrl `
                           -replace [regex]::Escape($templateFullName), $fullName `
                           -replace [regex]::Escape($TemplateName), $Name
    if ($_ -match 'csproj$') {
        $newContent = $newContent -replace '<Title>.*?</Title>', "<Title>EDR $Name Connector</Title>" `
                                  -replace '<Description>.*?</Description>', "<Description>A class library for using $Name in EDR Sequences</Description>"
    }
    [System.IO.File]::WriteAllText($path, $newContent, $encoding)
}

if (!$SkipReadme) {
    $readmePath = Join-Path (Get-Location).Path 'README.md'
    $readme = Get-Content $readmePath -Raw
    $newReadme = $readme -replace [regex]::Escape($TemplateUrl), $NewUrl `
                         -replace '# EDR Connector', "# EDR $Name Connector" `
                         -replace '\[Try Connector\]', "[Try $Name Connector]"
    [System.IO.File]::WriteAllText($readmePath, $newReadme, $encoding)
}

Rename-Item -Path "./$TemplateName/$TemplateName.csproj" -NewName "$Name.csproj"
Rename-Item -Path "./$TemplateName" -NewName $Name
Rename-Item -Path "./$TemplateName.Tests/$TemplateName.Tests.csproj" -NewName "$Name.Tests.csproj"
Rename-Item -Path "./$TemplateName.Tests" -NewName "$Name.Tests"

Rename-Item -Path "$TemplateName.sln.DotSettings" -NewName "$Name.sln.DotSettings"

Get-Item ./CHANGELOG.md -ErrorAction SilentlyContinue | Remove-Item

Remove-Item "$TemplateName.sln"
dotnet new sln -n $Name
dotnet sln add "$Name/$Name.csproj" "$Name.Tests/$Name.Tests.csproj"

# update packages
foreach ($p in @(
    "$Name/$Name.csproj"
    "$Name.Tests/$Name.Tests.csproj"
)) {
    $packages = dotnet list $p package --outdated
    $packages | Where-Object {
        $_ -match '^\s+> ([\w\.]+)(\s+((\d+\.){2}\d+)){3}'
    } | ForEach-Object {
        dotnet add $p package $Matches[1]
    }
}
