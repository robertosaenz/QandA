using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DbUp;
using API_QandA.Data;
using API_QandA.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API_QandA
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //DBUP
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            EnsureDatabase.For.SqlDatabase(connectionString);

            var upgrader = 
                DeployChanges.To
                .SqlDatabase(connectionString, null)
                .WithScriptsEmbeddedInAssembly(System.Reflection.Assembly.GetExecutingAssembly())
                .WithTransaction()
                .Build();

            if (upgrader.IsUpgradeRequired())
            {
                upgrader.PerformUpgrade();
            }
            //DBUP

            services.AddControllers();

            // IDataRepository is referenced in a constructor, substitute an instance of the DataRepository class.
            services.AddScoped<IDataRepository, DataRepository>();

            // CORS
            services.AddCors(options =>
                options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyMethod()
                .AllowAnyHeader()
                .WithOrigins("http://localhost:3000")
                .AllowCredentials()));


            // Signal R
            services.AddSignalR();

            // CACHE
            services.AddMemoryCache();
            services.AddSingleton<IQuestionCache, QuestionCache>();

            // AUTH0
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = Configuration["Auth0:Authority"];
                options.Audience = Configuration["Auth0:Audience"];
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // CORS
            app.UseCors("CorsPolicy");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseHttpsRedirection();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            //AUTH0
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Signal R
                endpoints.MapHub<QuestionsHub>("/questionshub");
            });
        }
    }
}
