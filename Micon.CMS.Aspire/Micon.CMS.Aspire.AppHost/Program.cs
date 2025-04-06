using Projects;

var builder = DistributedApplication.CreateBuilder(args);
var db = builder.AddPostgres("postgres")
    .AddDatabase("micon-cms-db","micondb");

builder.AddProject<Micon_CMS>("micon-cms")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
