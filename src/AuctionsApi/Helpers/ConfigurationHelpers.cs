using AuctionsApi.Business.Models.Abstract;
using AuctionsApi.Business.Models.Impl.Mongo;
using AuctionsApi.Business.Models.Objects;
using AuctionsApi.Models.Business.Abstract;
using AuctionsApi.Models.Business.Impl.Mongo;
using AuctionsApi.Models.Data.Abstract;
using AuctionsApi.Models.Data.Abstract.Mongo;
using AuctionsApi.Models.Data.Impl.Mongo;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.Swagger.Model;
using System;
using System.Collections.Generic;

namespace AuctionsApi.Helpers
{
    public static class ConfigurationHelpers
    {
        public static void AddMongoStore(this IServiceCollection services, IConfigurationRoot config)
        {
            services.AddTransient<IMongoDbContext, MongoDbContext>(p => new MongoDbContext(
                    config.GetConnectionString("MongoDb"),
                    config["MongoDbName"]));

            services.AddTransient<IAuctionSpecificationsFactory<AuctionDoc>, AuctionMongoSpecificationsFactory>();
            services.AddTransient<IRepository<ParticipantDoc>, ParticipantsMongoRepository>();
            services.AddTransient<IParticipantsService, ParticipantsMongoService>();

            if (config["Mode"].Equals("Demo"))
            {
                services.AddTransient<IRepository<AuctionDoc>, AuctionsMongoDemoRepository>();
            }
            else
            {
                services.AddTransient<IRepository<AuctionDoc>, AuctionsMongoRepository>();
            }

            services.AddTransient<IAuctionsService, AuctionsMongoService>();
            services.AddTransient<IMapper<AuctionDoc, AuctionSummary>, AuctionsMapper>();
        }

        public static void AddStubStore(this IServiceCollection services)
        {
            throw new NotImplementedException();
        }

        public static void AddSwagger(this IServiceCollection services, IConfigurationRoot config)
        {
            var apiScope = config["apiScope"];

            var authorityUrl = config["authorityUrl"];
            var apiKeyDefinition = config["swagger:apiKey:definition"];
            var apiKeyDescription = config["swagger:apiKey:description"];
            var apiKeyType = config["swagger:apiKey:type"];
            var apiKeyIn = config["swagger:apiKey:in"];
            var apiKeyName = config["swagger:apiKey:name"];

            var oauth2Definition = config["swagger:oauth2:definition"];
            var oauth2Scopes = config["swagger:oauth2:scopes"];
            var oauth2Flow = config["swagger:oauth2:flow"];
            var oauth2Type = config["swagger:oauth2:type"];
            var oauth2Path = config["swagger:oauth2:path"];
            var oauth2ScopesDescription = config["swagger:oauth2:scopes-description"];

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition(apiKeyDefinition, new ApiKeyScheme
                {
                    Description = apiKeyDescription,
                    Type = apiKeyType,
                    In = apiKeyIn,
                    Name = apiKeyName
                });

                c.AddSecurityDefinition(oauth2Definition, new OAuth2Scheme
                {
                    AuthorizationUrl = authorityUrl + oauth2Path,
                    Type = oauth2Type,
                    Flow = oauth2Flow,
                    Scopes = new Dictionary<string, string>
                    {
                        { string.Format("{0} {1}", oauth2Scopes, apiScope), oauth2ScopesDescription }
                    }
                });
            });
        }
    }
}
