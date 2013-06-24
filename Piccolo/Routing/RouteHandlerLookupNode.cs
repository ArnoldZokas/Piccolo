using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Piccolo.Routing
{
	[DebuggerDisplay("[{RouteTemplateFragment}], {ChildNodes.Count} child node(s)")]
	internal class RouteHandlerLookupNode
	{
		internal RouteHandlerLookupNode() : this(String.Empty, new List<string>(), null)
		{
		}

		internal RouteHandlerLookupNode(string headFragment, IEnumerable<string> routeTemplateFragments, Type requestHandler)
		{
			ChildNodes = new List<RouteHandlerLookupNode>();
			IsStaticRouteTemplateFragment = IsStaticFragment(headFragment);
			IsVirtualRouteTemplateFragment = IsVirtualFragment(routeTemplateFragments);
			RouteTemplateFragment = RemoveDynamicFragmentTokens(headFragment);
			RequestHandler = requestHandler;

			if (IsVirtualRouteTemplateFragment == false)
				RequestHandlerProperties = RouteHandlerDescriptor.GetRequestHandlerProperties(requestHandler);
		}

		internal IList<RouteHandlerLookupNode> ChildNodes { get; set; }

		internal bool IsStaticRouteTemplateFragment { get; set; }

		internal bool IsVirtualRouteTemplateFragment { get; set; }

		internal string RouteTemplateFragment { get; set; }

		internal Type RequestHandler { get; set; }

		internal Dictionary<string, Type> RequestHandlerProperties { get; set; }

		internal void AddNode(IList<string> routeTemplateFragments, Type requestHandler)
		{
			var headFragment = routeTemplateFragments.First();
			var remainingTemplateFragments = routeTemplateFragments.Skip(1).ToList();

			var childNode = FindChildNode(headFragment);
			if (childNode != null)
			{
				if (remainingTemplateFragments.Any())
					childNode.AddNode(remainingTemplateFragments, requestHandler);
				else
				{
					childNode.RequestHandler = requestHandler;
					childNode.RequestHandlerProperties = RouteHandlerDescriptor.GetRequestHandlerProperties(requestHandler);
				}
			}
			else
			{
				var newChildNode = new RouteHandlerLookupNode(headFragment, remainingTemplateFragments, requestHandler);
				ChildNodes.Add(newChildNode);
				if (remainingTemplateFragments.Any())
					newChildNode.AddNode(remainingTemplateFragments, requestHandler);
			}
		}

		private RouteHandlerLookupNode FindChildNode(string headFragment)
		{
			var searchTerm = RemoveDynamicFragmentTokens(headFragment);
			return ChildNodes.SingleOrDefault(x => x.RouteTemplateFragment == searchTerm);
		}

		private static bool IsStaticFragment(string headFragment)
		{
			return headFragment.Contains('{') == false;
		}

		private static bool IsVirtualFragment(IEnumerable<string> routeTemplateFragments)
		{
			return routeTemplateFragments.Any();
		}

		private static string RemoveDynamicFragmentTokens(string headFragment)
		{
			return headFragment.Trim(new[] {'{', '}'});
		}
	}
}