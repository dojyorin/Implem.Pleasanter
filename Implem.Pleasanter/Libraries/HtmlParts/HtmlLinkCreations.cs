﻿using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.DataSources;
using Implem.Pleasanter.Libraries.Html;
using Implem.Pleasanter.Libraries.Requests;
using Implem.Pleasanter.Libraries.Responses;
using Implem.Pleasanter.Libraries.Security;
using Implem.Pleasanter.Libraries.Server;
using Implem.Pleasanter.Libraries.Settings;
using Implem.Pleasanter.Models;
using System.Collections.Generic;
using System.Linq;
namespace Implem.Pleasanter.Libraries.HtmlParts
{
    public static class HtmlLinkCreations
    {
        public static HtmlBuilder LinkCreations(
            this HtmlBuilder hb,
            Context context,
            SiteSettings ss,
            long linkId,
            BaseModel.MethodTypes methodType,
            List<Link> links = null)
        {
            links = links ?? Links(
                context: context,
                ss: ss);
            return
                methodType != BaseModel.MethodTypes.New &&
                links.Any()
                    ? hb.FieldSet(
                        css: " enclosed link-creations is-sources",
                        legendText: Displays.LinkCreations(context: context),
                        action: () => hb
                            .LinkCreations(
                                context: context,
                                ss: ss,
                                links: links,
                                linkId: linkId))
                    : hb;
        }

        public static List<Link> Links(Context context, SiteSettings ss)
        {
            var links = new LinkCollection(
                context: context,
                column: Rds.LinksColumn()
                    .SourceId()
                    .DestinationId()
                    .SiteTitle(),
                where: Rds.LinksWhere()
                    .DestinationId(ss.SiteId)
                    .SiteId_In(ss.Sources
                        ?.Values
                        .Where(currentSs => context.CanCreate(ss: currentSs))
                        .Select(currentSs => currentSs.SiteId)))
                            .Select(linkModel => GetLink(
                                linkModel: linkModel,
                                ss: ss.Sources.Get(linkModel.SourceId)))
                            .Where(o => o != null)
                            .ToList();
            var sortedSources = GetSortedSources(ss: ss).Keys.ToList();
            return links
                .OrderBy(link => sortedSources.IndexOf(link.SourceId))
                .ToList();
        }

        private static Dictionary<long, SiteSettings> GetSortedSources(SiteSettings ss)
        {
            return ss.Sources?.Any(o => o.Value.Links
                .Where(p => p.SiteId == ss.SiteId)
                .Any(p => p.Priority != null)) == true
                    ? ss.Sources
                        ?.OrderBy(o => o.Value.Links
                            .Where(p => p.SiteId == ss.SiteId)
                            .Select(p => p.Priority > 0
                                ? p.Priority
                                : int.MaxValue)
                            .Min())
                        .ThenBy(o => o.Key)
                        .ToDictionary(o => o.Key, o => o.Value)
                    : ss.Sources ?? new Dictionary<long, SiteSettings>();
        }

        private static Link GetLink(LinkModel linkModel, SiteSettings ss)
        {
            var link = ss?.Links
                .Where(o => o.SiteId > 0)
                .Where(o => o.NoAddButton != true)
                .FirstOrDefault(o => o.SiteId == linkModel.DestinationId);
            if (link != null)
            {
                link.SiteTitle = linkModel.SiteTitle;
                link.SourceId = linkModel.SourceId;
            }
            return link;
        }

        private static HtmlBuilder LinkCreations(
            this HtmlBuilder hb,
            Context context,
            SiteSettings ss,
            List<Link> links,
            long linkId)
        {
            return hb.Div(action: () => links.ForEach(link => hb
                .LinkCreationButton(
                    context: context,
                    ss: ss,
                    linkId: linkId,
                    sourceId: link.SourceId,
                    text: link.SiteTitle,
                    notReturnParentRecord: link.NotReturnParentRecord ?? false)));
        }

        public static HtmlBuilder LinkCreationButton(
            this HtmlBuilder hb,
            Context context,
            SiteSettings ss,
            long linkId,
            long sourceId,
            string text,
            int tabIndex = 0,
            bool? notReturnParentRecord = false)
        {
            return hb.Button(
                attributes: new HtmlAttributes()
                    .Class("button button-icon confirm-unload")
                    .OnClick("$p.new($(this));")
                    .Title(SiteInfo.TenantCaches.Get(context.TenantId)?
                        .SiteMenu
                        .Breadcrumb(context: context, siteId: sourceId)
                        .Select(o => o.Title).Join(" > "))
                    .DataId(linkId.ToString())
                    .DataIcon("ui-icon-plus")
                    .Add("data-from-site-id", ss.SiteId.ToString())
                    .Add("data-to-site-id", sourceId.ToString())
                    .Add(
                        name: "from-tab-index",
                        value: tabIndex.ToString())
                    .Add("do-not-return-parent",
                        notReturnParentRecord.ToString().ToLower()),
                action: () => hb
                    .Text(text: text));
        }
    }
}