using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Micon.CMS.Models;

namespace Micon.CMS
{
    public class CustomNpgsqlMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
    {
        public CustomNpgsqlMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            INpgsqlSingletonOptions typeMappingSource
        ) : base(dependencies, typeMappingSource) { }

        protected override void Generate(CreateTableOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
        {
            // 通常のテーブル作成SQLを生成
            base.Generate(operation, model, builder, terminate);

            if(operation.Name.ToLower() != "__EFMigrationsHistory" && operation.Name != "Tenants" && operation.Name != "ApplicationRoles" && operation.Name != "Tenants" && operation.Name != "ApplicationUsers")
            {
                // RLS を適用する SQL を追加

                builder.AppendLine($@"
                ALTER TABLE {operation.Name} ENABLE ROW LEVEL SECURITY;
                CREATE POLICY tenant_policy
                ON {operation.Name}
                FOR SELECT, INSERT, UPDATE, DELETE
                USING (TenantId = current_setting('app.current_tenant'::UUID));
            ");
            }
            else
            {

            }

        }
    }
}
