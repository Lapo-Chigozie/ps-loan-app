using Google.Protobuf.WellKnownTypes;
using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.LoanScheduled.Model;
using LapoLoanWebApi.LoanScheduled.UploadHostedServices;
using LapoLoanWebApi.Service;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using ServiceStack;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRazorPages();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddCors();
//builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lapo Loan Service", Version = "v1", });
//    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//    //c.IncludeXmlComments(xmlPath);
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: LapoLoanAllowSpecificOrigins.OriginNamme1,
                      policy =>
                      {
                          policy.WithOrigins(DefalutToken.AllowOrignUrl)
                          .WithMethods(DefalutToken.OrignMethods)
                          //.SetIsOriginAllowedToAllowWildcardSubdomains()
                          //.SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
                          //.WithExposedHeaders("x-custom-header")
                          .AllowAnyHeader()
                         // ./*AllowCredentials()*/
                         .AllowAnyMethod();
                          //.AllowAnyOrigin();
                          //.WithHeaders(HeaderNames.AccessControlAllowHeaders, "Content-Type");
     
                      });
});

//builder.Services.AddHostedService<FileUploadHostedServices>();
builder.Services.AddHostedService<TimedHostedService>();

//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.Limits.MaxRequestBodySize = 52428800; //50MB
//});

//builder.Services.Configure<IISServerOptions>(options =>
//{
//    options.MaxRequestBodySize = 52428800; //50MB
//});

//builder.Services.Configure<FormOptions>(o =>
//{
//    o.ValueLengthLimit = int.MaxValue;
//    o.MultipartBodyLengthLimit = long.MaxValue;
//    o.MemoryBufferThreshold = int.MaxValue;
//    o.MultipartHeadersLengthLimit = int.MaxValue;

//    o.MultipartBoundaryLengthLimit = int.MaxValue;
//    o.MultipartHeadersCountLimit = int.MaxValue;

//    //o.BufferBodyLengthLimit = long.MaxValue;
//    //o.BufferBody = true;
//    //o.ValueCountLimit = int.MaxValue;

//});

//builder.Services.Configure<KestrelServerOptions>(options =>
//{
//    options.Limits.MaxRequestBodySize = long.MaxValue; // if don't set default value is: 30 MB
//});


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(LapoLoanAllowSpecificOrigins.OriginNamme1,
//                          policy =>
//                          {
//                              policy.WithOrigins("http://localhost:4200",
//                                                  "https://192.168.7.90:45455")
//                                                  .AllowAnyHeader()
//                                                  .AllowAnyMethod()
//                                                    .WithMethods("PUT", "DELETE", "GET")
//                     .SetIsOriginAllowedToAllowWildcardSubdomains()
//              .SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
//             .AllowCredentials()
//                   .WithExposedHeaders("x-custom-header");
//                          });
//});


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(name: LapoLoanAllowSpecificOrigins.OriginNamme1,
//        policy =>
//        {
//            policy.WithOrigins("http://localhost:4200",
//                                "https://192.168.7.90:45455")
//                    .WithMethods("PUT", "DELETE", "GET")
//                     .SetIsOriginAllowedToAllowWildcardSubdomains()
//              .SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
//             .AllowCredentials()
//                   .WithExposedHeaders("x-custom-header");
//        });
//});


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(name: LapoLoanAllowSpecificOrigins.OriginNamme1,
//        policy =>
//        {
//            policy.WithOrigins("https://192.168.7.90:45455")
//                .SetIsOriginAllowedToAllowWildcardSubdomains()
//              .SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
//             .AllowCredentials()
//                   .WithExposedHeaders("x-custom-header");
//        });
//});


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(name: LapoLoanAllowSpecificOrigins.OriginNamme1,
//       policy =>
//       {
//           policy.WithOrigins("https://192.168.7.90:45455")
//                  .WithHeaders(HeaderNames.ContentType, "x-custom-header");
//       });
//});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(LapoLoanAllowSpecificOrigins.OriginNamme1,
//        policy =>
//        {
//            policy.WithOrigins("https://192.168.7.90:45455")
//             .SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
//             .AllowCredentials()
//                   .WithExposedHeaders("x-custom-header");
//        });
//});

//builder.Services.Configure<FormOptions>(o =>
//{
//    o.ValueLengthLimit = int.MaxValue;
//    o.MultipartBodyLengthLimit = int.MaxValue;
//    o.MemoryBufferThreshold = int.MaxValue;
//});


var app = builder.Build();

//app.UseSecurityHeadersMiddleware(new SecurityHeadersBuilder()
//    .AddDefaultSecurePolicy()
//    .AddCustomHeader("Access-Control-Allow-Origin", "http://localhost:3000")
//    .AddCustomHeader("Access-Control-Allow-Methods", "OPTIONS, GET, POST, PUT, PATCH, DELETE")
//    .AddCustomHeader("Access-Control-Allow-Headers", "X-PINGOTHER, Content-Type, Authorization"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwaggerGen();
    app.UseSwagger();
    app.UseSwaggerUI();
   
   // app.UseDeveloperExceptionPage();
    //app.UseSwaggerUI(c =>
    //{
    //    foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
    //    {
    //        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    //    }

    //});
    //app.UseSwaggerUI(c =>
    //{
    //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DOA.API V1");
    //});
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwaggerUI(c =>
{
   // c.SwaggerEndpoint("swagger/v1/swagger.json", "V1 LAPO LOAN APP");

    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tinroll API v0.1 LAPO LOAN APP");

    //c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")), RequestPath = new PathString("/Resources")
});

app.UseRouting();
app.UseCors(LapoLoanAllowSpecificOrigins.OriginNamme1);
app.UseAuthorization();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapGet("/echo",
//        context => context.Response.WriteAsync("echo"))
//        .RequireCors(LapoLoanAllowSpecificOrigins.OriginNamme1);

//    endpoints.MapControllers()
//             .RequireCors(LapoLoanAllowSpecificOrigins.OriginNamme1);

//    endpoints.MapGet("/echo2",
//        context => context.Response.WriteAsync("echo2"));

//    endpoints.MapRazorPages();
//});

//app.UseEndpoints(endpoints => {
//    endpoints.MapControllers();
//    endpoints.MapRazorPages();
//});




app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
