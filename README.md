# GithubActions For extract json properties from file example:


    - name: dotnet-json-extractorcs
      id: dotnet-json-extractorcs
      uses: chrisfactory/GithubActions@1.0.0.2
      with:
        path: .github/workflows/parameters/example.json
        properties: 'contentVersion;parameters.registryLocation;parameters.registryName.value'
        propertiesAlias: 'contentVersion:version;parameters.registryName.value:registryName'
         

    - run: echo ${{ steps.dotnet-json-extractorcs.outputs.parameters-registryLocation }}
    - run: echo ${{ steps.dotnet-json-extractorcs.outputs.registryName}}
    - run: echo ${{ steps.dotnet-json-extractorcs.outputs.version }} 
