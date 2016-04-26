﻿using EPiServer.Core;
using EPiServer.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TcbInternetSolutions.Vulcan.Core.Implementation
{
    public class VulcanClient : ElasticClient, IVulcanClient
    {
        private static ILogger Logger = LogManager.GetLogger();

        public VulcanClient(ConnectionSettings settings) : base(settings)
        {
        }

        public ISearchResponse<IContent> SearchContent<T>(Func<SearchDescriptor<T>, SearchDescriptor<T>> searchDescriptor = null) where T : class, EPiServer.Core.IContent
        {
            SearchDescriptor<T> resolvedDescriptor;

            if (searchDescriptor == null)
            {
                resolvedDescriptor = new SearchDescriptor<T>();
            }
            else
            {
                resolvedDescriptor = searchDescriptor.Invoke(new SearchDescriptor<T>());
            }

            var types = new List<string>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                types.AddRange(assembly.GetTypes().Where(t => typeof(T).IsAssignableFrom(t)).Select(t => t.FullName));
            }

            resolvedDescriptor = resolvedDescriptor.Type(string.Join(",", types)).ConcreteTypeSelector((d, docType) => typeof(VulcanContentHit));

            Func<SearchDescriptor<T>, ISearchRequest> selector = ts => resolvedDescriptor;

            return base.Search<T, IContent>(selector);
        }

        public void IndexContent(IContent content)
        {
            if (!(content is IVersionable) || (content as IVersionable).Status == VersionStatus.Published)
            {
                try
                {
                    var response = base.Index(content, c => c.Id(content.ContentLink.ToString()).Type(GetTypeName(content)));
                }
                catch (Exception e)
                {
                    Logger.Warning("Vulcan could not index content with content link: " + content.ContentLink.ToString(), e);
                }
            }
        }

        public void DeleteContent(IContent content)
        {
            try
            {
                var response = base.Delete(new DeleteRequest(VulcanHelper.Index, GetTypeName(content), content.ContentLink.ToString()));
            }
            catch (Exception e)
            {
                Logger.Warning("Vulcan could not delete content with content link: " + content.ContentLink.ToString(), e);
            }
        }

        private string GetTypeName(IContent content)
        {
            return content.GetType().Name.EndsWith("Proxy") ? content.GetType().BaseType.FullName : content.GetType().FullName;
        }
    }
}