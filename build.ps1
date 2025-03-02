<#
.Synopsis
    Build script <https://github.com/nightroman/Invoke-Build>

.Example
    PS> ./Novell.Directory.Ldap.NetStandard.build.ps1 build -Configuration Release
#>

param(
    [Parameter(Position=0)]
    [string[]]$Tasks,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    # stress tests params
    [ValidateSet('net9', 'net8', 'net6')]
    [string]$Fx = 'net8',
    [string]$ConcurrencyLevel = 20,
    [ValidateSet('off', 'tls', 'ssl')]
    [string]$TransportSecurity = 'off'
)

# Ensure and call the module.
if ([System.IO.Path]::GetFileName($MyInvocation.ScriptName) -ne 'Invoke-Build.ps1') {
    $InvokeBuildVersion = '5.11.0'
    $ErrorActionPreference = 'Stop'
    try {
        Import-Module InvokeBuild -RequiredVersion $InvokeBuildVersion
    }
    catch {
        Install-Module InvokeBuild -RequiredVersion $InvokeBuildVersion -Scope CurrentUser -Force
        Import-Module InvokeBuild -RequiredVersion $InvokeBuildVersion
    }
    Invoke-Build -Task $Tasks -File $MyInvocation.MyCommand.Path @PSBoundParameters
    return
}

$script:SupportedNetVersions = @(
    'net9'
    'net8'
    'net6'
)

task build {
    exec {
        dotnet build -c $Configuration
    }
}

task test-unit {
    foreach($netVersion in $SupportedNetVersions) {
        exec {
            dotnet test --configuration $Configuration --no-build `
                test/Novell.Directory.Ldap.NETStandard.UnitTests/Novell.Directory.Ldap.NETStandard.UnitTests.csproj -f $netVersion `
                 -l "console;verbosity=normal"
        }
    }
}

task configure-opendj {
    exec { whoami }

    exec {
        # run openjd in docker
        docker run -d -h ldap-01.example.com -p 4389:1389 -p 4636:1636 -p 4444:4444 --name opendj --env-file opendj-docker-env.props openidentityplatform/opendj
    }

    exec {
        # give openldap enough time to start
        sleep 30
        docker ps -a
    }
}

task internal-test-functional {
    foreach($netVersion in $SupportedNetVersions) {
        Write-Build "Running functional tests on $netVersion platform" -Color Magenta
        exec {
            dotnet test --configuration $CONFIGURATION  --no-build `
                test/Novell.Directory.Ldap.NETStandard.FunctionalTests/Novell.Directory.Ldap.NETStandard.FunctionalTests.csproj -f $netVersion `
                 -l "console;verbosity=normal"
        }
    }
}

task test-functional configure-opendj, configure-openldap, {
    $transportSecurityOptions = @("OFF", "SSL", "TLS")

    foreach($transportSecurity in $transportSecurityOptions) {
        $env:TRANSPORT_SECURITY=$transportSecurity
        Write-Build "Running functional tests with security $($env:TRANSPORT_SECURITY)" -Color Magenta
        
        Invoke-Build internal-test-functional $BuildFile
    }
}

task remove-opendj -After test-functional {
    exec {
        docker kill opendj
    }
    exec {
        docker rm opendj
    }
}

task test test-unit, test-functional, {
}

task configure-openldap {
    exec {
        sudo apt-get update
    }
    exec {
        sudo DEBIAN_FRONTEND=noninteractive apt-get install `
          ldap-utils gnutls-bin ssl-cert slapd `
          sasl2-bin libsasl2-2 libsasl2-modules libsasl2-modules-ldap `
          -y
    }
    exec {
        bash configure-openldap.sh
    }
}

task remove-openldap -After test-stress {
    exec {
        bash build/remove-openldap.sh
    }
}

task test-stress configure-openldap, {
    $env:TRANSPORT_SECURITY=$TransportSecurity.ToUpper()
    exec {
        dotnet run --configuration $CONFIGURATION `
            --project test/Novell.Directory.Ldap.NETStandard.StressTests/Novell.Directory.Ldap.NETStandard.StressTests.csproj `
            $ConcurrencyLevel 30 -f $Fx `
            -l "console;verbosity=normal"
    }
}

task clean {
    remove bin, obj
}

task . build, test
task reset-openldap remove-openldap, configure-openldap