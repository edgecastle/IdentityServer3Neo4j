# Neo4j Graph database providers for IdentityServer3 #

## Getting started ##
Simply add the Edgecastle.IdentityServer3.Neo4j nuget package to your project.

When adding [IdentityServer](http://github.com/identityserver/IdentityServer3) use the Neo4j factory in the IdentityServerOptions:

```csharp
public void Configuration(IAppBuilder app)
{
    var options = new IdentityServerOptions
    {
        SigningCertificate = Certificate.Get(),
        Factory = Neo4jServiceFactory.Create()
    };

    app.UseIdentityServer(options);

	...
}
```

For more information, release notes, etc. view [http://edgecastle.github.io/IdentityServer3Neo4j/](http://edgecastle.github.io/IdentityServer3Neo4j/)
