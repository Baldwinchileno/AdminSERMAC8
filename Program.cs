using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AdminSERMAC.Core.Configuration;
using AdminSERMAC.Services;
using AdminSERMAC.Repositories;
using AdminSERMAC.Core.Interfaces;
using System.Windows.Forms;
using System;

namespace AdminSERMAC
{
    public class ProgramLogger { } // Clase auxiliar para logging

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Configurar servicios
            var services = new ServiceCollection();
            var connectionString = "Data Source=AdminSERMAC.db;Version=3;";

            // Registrar los servicios
            RegisterServices(services, connectionString);

            // Construir el proveedor de servicios
            var serviceProvider = services.BuildServiceProvider();

            try
            {
                // Obtener los servicios necesarios
                var clienteService = serviceProvider.GetRequiredService<IClienteService>();

                // Iniciar la aplicación
                Application.Run(new MainForm(clienteService, connectionString));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar la aplicación: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                var logger = serviceProvider.GetRequiredService<ILogger<ProgramLogger>>();
                logger.LogError(ex, "Error fatal al iniciar la aplicación");
            }
        }

        private static void RegisterServices(IServiceCollection services, string connectionString)
        {
            // Configurar logging
            services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.AddDebug();
                configure.SetMinimumLevel(LogLevel.Information);
            });

            // Registrar SQLiteService
            services.AddSingleton<SQLiteService>();

            // Registrar ClienteRepository
            services.AddScoped<IClienteRepository>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<ClienteRepository>>();
                return new ClienteRepository(connectionString, logger);
            });

            // Registrar ClienteService
            services.AddScoped<IClienteService>(provider =>
            {
                var repository = provider.GetRequiredService<IClienteRepository>();
                var logger = provider.GetRequiredService<ILogger<ClienteService>>();
                return new ClienteService(repository, logger, connectionString);
            });

            // Registrar servicios adicionales
            services.AddSingleton<NotificationService>();
            services.AddScoped<FileDataManager>();
            services.AddSingleton<ConfigurationService>();
            services.AddSingleton<DatabaseInitializer>();
        }
    }
}