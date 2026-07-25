# Mappings

Object-to-object mapping profiles. No mapper library is wired yet (the current
handlers map by hand). When the first real mapping is needed, add AutoMapper:

```
dotnet add package AutoMapper
// then in AddApplicationServices(): services.AddAutoMapper(assembly);
```
and place `Profile` classes here.
