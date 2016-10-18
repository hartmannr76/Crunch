using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Crunch.Attributes;

namespace Crunch
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddLogging();
            
            var assemblies = FindAssemblies();
            
            Console.Out.WriteLine(string.Format("{0} assemblies found", assemblies.Count));
            var holderassemblies = assemblies.ToList();
            holderassemblies.Add(Assembly.GetEntryAssembly());
            var registeredClasses = FindAttributeRegisteredClasses(holderassemblies);
            Console.Out.WriteLine(string.Format("{0} classes to register", registeredClasses.Count()));
            foreach(var item in registeredClasses) {
                Console.Out.WriteLine(string.Format("Registering {0} for {1}", item.Item1.ToString(), item.Item2.ToString()));
                RegisterTypeForLifetime(services, item.Item1, item.Item2);
            }
        }
        
        private ImmutableList<Assembly> FindAssemblies() {
            var ctx = DependencyContext.Default;
            var ignoredAssemblies = new HashSet<string>() { "Microsoft.", "System." };
            
            var assemblyNames = from lib in ctx.RuntimeLibraries
                                from assemblyName in lib.GetDefaultAssemblyNames(ctx)
                                where ignoredAssemblies.Any(x => assemblyName.Name.StartsWith(x))
                                select assemblyName;
            var lookup = assemblyNames.ToLookup(x => x.Name).Select(x => x.First());
            var asList = lookup.Select(Assembly.Load).ToImmutableList();
            return asList;
        }
        
        private IEnumerable<Tuple<Type, Crunch.Attributes.ServiceLifetime>> FindAttributeRegisteredClasses(IEnumerable<Assembly> assemblies) {
            
            return
                from assembly in assemblies
                from type in assembly.DefinedTypes
                let attr = type.GetCustomAttribute<AutoRegisterAttribute>()
                where attr != null
                let lifetime = attr.ServiceLifetime
                select Tuple.Create(type.AsType(), lifetime);
        }
        
        private void RegisterTypeForLifetime(IServiceCollection collection, Type type, Crunch.Attributes.ServiceLifetime lifetime) {
            var interfaces = type.GetInterfaces();
            
            foreach(var face in interfaces) {
                if(lifetime == Crunch.Attributes.ServiceLifetime.Singleton) {
                    collection.AddSingleton(face, type);
                } else if(lifetime == Crunch.Attributes.ServiceLifetime.PerRequest) {
                    collection.AddScoped(face, type);
                } else {
                    collection.AddTransient(face, type);
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
