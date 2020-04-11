using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CovidInfo.Repository.Settings;
using CovidInfo.Services;
using CovidInfo.Services.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CovidInfo {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            services.Configure<CovidInfoDatabaseSettings>(
                Configuration.GetSection(nameof(CovidInfoDatabaseSettings)));

            services.AddCors(options =>
             options.AddDefaultPolicy(builder => builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

            services.AddSingleton<ICovidInfoDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<CovidInfoDatabaseSettings>>().Value);

            services.AddSingleton<CovidService>();

            services.AddHttpClient();
          
            services.AddSignalR();


            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapHub<CovidInfoHub>("/covidHub");
            });
        }
    }
}
