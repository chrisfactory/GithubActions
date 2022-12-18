# GithubActions For extract json properties from file example:
    - name: dotnet-json-extractorcs
      id: dotnet-json-extractorcs
      uses: /chrisfactory/GithubActions@master
      with:
        path: .github/workflows/parameters/example.json
        properties: 'contentVersion;parameters.registryName;parameters.registryName.value'
         

    - run: echo ${{ steps.dotnet-json-extractorcs.outputs.json-values-parameters-registryName }}
    - run: echo ${{ steps.dotnet-json-extractorcs.outputs.json-values-parameters-registryName-value }}
    - run: echo ${{ steps.dotnet-json-extractorcs.outputs.json-values-contentVersion }} 
