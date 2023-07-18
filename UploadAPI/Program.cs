using UploadAPI.Infrastructure;
using UploadAPI.Models.Configs;

namespace UploadAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<FtpConfig>(builder.Configuration.GetSection("Ftp"));
            builder.Services.Configure<List<FileSizeConfig>>(builder.Configuration.GetSection("FileMaxSizes"));
            builder.Services.Configure<List<string>>(builder.Configuration.GetSection("AcceptFiles"));
            builder.Services.Configure<List<UploadTypeConfig>>(builder.Configuration.GetSection("UploadTypes"));
            builder.Services.AddTransient<IUploadService, UploadService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}