﻿using Implem.DefinitionAccessor;
using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.Extensions;
using Implem.Pleasanter.Libraries.Html;
using Implem.Pleasanter.Libraries.HtmlParts;
using Implem.Pleasanter.Libraries.Requests;
using Implem.Pleasanter.Libraries.Responses;
using Implem.Pleasanter.Libraries.Server;
using Implem.Pleasanter.Libraries.Settings;
using Implem.Pleasanter.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Implem.Pleasanter.Libraries.Models
{
    public static class DropDowns
    {
        public static string SearchDropDown(
            Context context,
            SiteSettings ss)
        {
            var controlId = context.Forms.Data("DropDownSearchTarget");
            var referenceId = context.Forms.Long("DropDownSearchReferenceId");
            var filter = controlId.StartsWith("ViewFilters__")
                || controlId.StartsWith("ProcessViewFilters__")
                || controlId.StartsWith("StatusControlViewFilters__")
                || controlId.StartsWith("DashboardViewFilters__")
                || controlId.StartsWith("ViewFiltersOnGridHeader__");
            var searchText = context.Forms.Data("DropDownSearchText");
            string parentClass = context.Forms.Data("DropDownSearchParentClass");
            var parentDataId = context.Forms.Data("DropDownSearchParentDataId");
            var parentIds = parentDataId.Deserialize<List<long>>();
            switch (context.Forms.ControlId())
            {
                case "DropDownSearchText":
                    return SearchDropDownSelectable(
                        context: context,
                        ss: ss,
                        controlId: controlId,
                        referenceId: referenceId,
                        searchText: searchText,
                        filter: filter,
                        parentClass: parentClass,
                        parentIds: parentIds);
                case "DropDownSearchResults":
                case "DropDownSearchSourceResults":
                    return AppendSearchDropDownSelectable(
                        context: context,
                        ss: ss,
                        controlId: controlId,
                        referenceId: referenceId,
                        searchText: searchText,
                        filter: filter,
                        parentClass: parentClass,
                        parentIds: parentIds);
                default:
                    return SearchDropDown(
                        context: context,
                        ss: ss,
                        controlId: controlId,
                        referenceId: referenceId,
                        filter: filter,
                        parentClass: parentClass,
                        parentIds: parentIds);
            }
        }

        private static string SearchDropDown(
            Context context,
            SiteSettings ss,
            string controlId,
            long referenceId,
            bool filter,
            string parentClass = "",
            List<long> parentIds = null)
        {
            switch (controlId)
            {
                case "InheritPermission":
                    return SearchInheritPermissionDropDown(
                        context: context,
                        ss: ss);
                default:
                    return SearchCommonDropDown(
                        context: context,
                        ss: ss,
                        controlId: controlId,
                        referenceId: referenceId,
                        filter: filter,
                        parentClass: parentClass,
                        parentIds: parentIds);
            }
        }

        private static string SearchInheritPermissionDropDown(
            Context context,
            SiteSettings ss)
        {
            var nextOffset = Paging.NextOffset(
                offset: 0,
                totalCount: PermissionUtilities.InheritTargets(context, ss).TotalCount,
                pageSize: Parameters.General.DropDownSearchPageSize);
            return new ResponseCollection(context: context)
                .Html(
                    "#DropDownSearchDialogBody",
                    new HtmlBuilder().DropDownSearchDialogBodyInheritPermission(
                        context: context,
                        ss: ss,
                        offset: 0,
                        pageSize: Parameters.General.DropDownSearchPageSize))
                .Val("#DropDownSearchResultsOffset", nextOffset)
                .ClearFormData("DropDownSearchResults")
                .ToJson();
        }

        private static string SearchCommonDropDown(
            Context context,
            SiteSettings ss,
            string controlId,
            long referenceId,
            bool filter,
            string parentClass,
            List<long> parentIds)
        {
            var column = SearchDropDownColumn(
                context: context,
                ss: ss,
                controlId: controlId,
                referenceId: referenceId,
                searchText: string.Empty,
                parentClass: parentClass,
                parentIds: parentIds);
            if (!parentClass.IsNullOrEmpty() && (parentIds?.Any() ?? false) == false)
            {
                column.ChoiceHash.Clear();
            }
            var nextOffset = Paging.NextOffset(
                offset: 0,
                totalCount: column.TotalCount,
                pageSize: Parameters.General.DropDownSearchPageSize);
            return new ResponseCollection(context: context)
                .Html(
                    "#DropDownSearchDialogBody",
                    new HtmlBuilder().DropDownSearchDialogBody(
                        context: context,
                        column: column,
                        filter: filter))
                .Val("#DropDownSearchResultsOffset", nextOffset)
                .ClearFormData("DropDownSearchResults")
                .ToJson();
        }

        private static string SearchDropDownSelectable(
            Context context,
            SiteSettings ss,
            string controlId,
            long referenceId,
            string searchText,
            bool filter,
            string parentClass = "",
            List<long> parentIds = null)
        {
            switch (controlId)
            {
                case "InheritPermission":
                    return SearchInheritPermissionDropDownSelectable(
                        context: context,
                        ss: ss,
                        searchText: searchText);
                default:
                    return SearchCommonDropDownSelectable(
                        context: context,
                        ss: ss,
                        controlId: controlId,
                        referenceId: referenceId,
                        searchText: searchText,
                        filter: filter,
                        parentClass: parentClass,
                        parentIds: parentIds);
            }
        }

        private static string SearchInheritPermissionDropDownSelectable(
            Context context,
            SiteSettings ss,
            string searchText)
        {
            var (optionCollection, totalCount) = PermissionUtilities.InheritTargets(
                context: context,
                ss: ss,
                offset: 0,
                pageSize: Parameters.General.DropDownSearchPageSize,
                searchText: searchText);
            var nextOffset = Paging.NextOffset(
                offset: 0,
                totalCount: totalCount,
                pageSize: Parameters.General.DropDownSearchPageSize);
            return new ResponseCollection(context: context)
                .Html(
                    "#DropDownSearchResults",
                    new HtmlBuilder().SelectableItems(
                        listItemCollection: optionCollection,
                        alwaysDataValue: true))
                .Val("#DropDownSearchResultsOffset", nextOffset)
                .ClearFormData("DropDownSearchResults")
                .ToJson();
        }

        private static string SearchCommonDropDownSelectable(
            Context context,
            SiteSettings ss,
            string controlId,
            long referenceId,
            string searchText,
            bool filter,
            string parentClass,
            List<long> parentIds)
        {
            var column = SearchDropDownColumn(
                context: context,
                ss: ss,
                controlId: controlId,
                referenceId: referenceId,
                searchText: searchText,
                parentClass: parentClass,
                parentIds: parentIds);
            var nextOffset = Paging.NextOffset(
                offset: 0,
                totalCount: column.TotalCount,
                pageSize: Parameters.General.DropDownSearchPageSize);
            var selectedValues = (column?.MultipleSelections == true || filter)
                ? context.Forms.List("DropDownSearchResultsAll")
                : new List<string>();
            return new ResponseCollection(context: context)
                .Html(
                    (column?.MultipleSelections == true || filter)
                        ? "#DropDownSearchSourceResults"
                        : "#DropDownSearchResults",
                    new HtmlBuilder().SelectableItems(
                        listItemCollection: column?.EditChoices(
                            context: context,
                            addNotSet: true,
                            own: filter)
                                .Where(o => !selectedValues.Contains(o.Key))
                                .ToDictionary(o => o.Key, o => o.Value),
                        alwaysDataValue: true))
                .Val("#DropDownSearchResultsOffset", nextOffset)
                .ClearFormData("DropDownSearchResults")
                .ToJson();
        }

        private static string AppendSearchDropDownSelectable(
            Context context,
            SiteSettings ss,
            string controlId,
            long referenceId,
            string searchText,
            bool filter,
            string parentClass = "",
            List<long> parentIds = null)
        {
            switch (controlId)
            {
                case "InheritPermission":
                    return AppendSearchInheritPermissionDropDownSelectable(
                        context: context,
                        ss: ss,
                        searchText: searchText);
                default:
                    return AppendSearchCommonDropDownSelectable(
                        context: context,
                        ss: ss,
                        controlId: controlId,
                        referenceId: referenceId,
                        searchText: searchText,
                        filter: filter,
                        parentClass: parentClass,
                        parentIds: parentIds);
            }
        }

        private static string AppendSearchInheritPermissionDropDownSelectable(
            Context context,
            SiteSettings ss,
            string searchText)
        {
            var offset = context.Forms.Int("DropDownSearchResultsOffset");
            var (optionCollection, totalCount) = PermissionUtilities.InheritTargets(
                context: context,
                ss: ss,
                offset: offset,
                pageSize: Parameters.General.DropDownSearchPageSize,
                searchText: searchText);
            var nextOffset = Paging.NextOffset(
                offset: offset,
                totalCount: totalCount,
                pageSize: Parameters.General.DropDownSearchPageSize);
            return new ResponseCollection(context: context)
                .Append(
                    "#" + context.Forms.ControlId(),
                    new HtmlBuilder().SelectableItems(optionCollection))
                .Val("#DropDownSearchResultsOffset", nextOffset)
                .ToJson();
        }

        private static string AppendSearchCommonDropDownSelectable(
            Context context, SiteSettings ss,
            string controlId,
            long referenceId,
            string searchText,
            bool filter,
            string parentClass,
            List<long> parentIds)
        {
            var offset = context.Forms.Int("DropDownSearchResultsOffset");
            var column = SearchDropDownColumn(
                context: context,
                ss: ss,
                controlId: controlId,
                referenceId: referenceId,
                searchText: searchText,
                offset: offset,
                parentClass: parentClass,
                parentIds: parentIds);
            var nextOffset = Paging.NextOffset(
                offset: offset,
                totalCount: column.TotalCount,
                pageSize: Parameters.General.DropDownSearchPageSize);
            var selectedValues = column?.MultipleSelections == true
                ? context.Forms.List("DropDownSearchResultsAll")
                : new List<string>();
            return new ResponseCollection(context: context)
                .Append(
                    "#" + context.Forms.ControlId(),
                    new HtmlBuilder().SelectableItems(
                        listItemCollection: column?.EditChoices(
                            context: context,
                            addNotSet: offset == 0,
                            own: filter)
                                .Where(o => !selectedValues.Contains(o.Key))
                                .ToDictionary(o => o.Key, o => o.Value)))
                .Val("#DropDownSearchResultsOffset", nextOffset)
                .ToJson();
        }

        public static string RelatingDropDown(
            Context context,
            SiteSettings ss)
        {
            var controlId = context.Forms.Data("RelatingDropDownControlId");
            var referenceId = context.Forms.Long("DropDownSearchReferenceId");
            var filter = controlId.StartsWith("ViewFilters__")
                || controlId.StartsWith("ViewFiltersOnGridHeader__");
            string parentClass = context.Forms.Data("RelatingDropDownParentClass");
            var selectedValue = context.Forms.Data("RelatingDropDownSelected");
            var parentDataId = context.Forms.Data("RelatingDropDownParentDataId");
            var isInitDisplay = context.Forms.Bool("IsInitDisplay");
            var parentIds = parentDataId.Deserialize<List<long>>();
            return RelatingDropDown(
                context: context,
                ss: ss,
                controlId: controlId,
                referenceId: referenceId,
                selectedValue: selectedValue,
                searchText: string.Empty,
                filter: filter,
                isInitDisplay: isInitDisplay,
                parentClass: parentClass,
                parentIds: parentIds);
        }

        private static string RelatingDropDown(
            Context context,
            SiteSettings ss,
            string controlId,
            long referenceId,
            string searchText,
            string selectedValue,
            bool filter,
            bool isInitDisplay,
            string parentClass = "",
            List<long> parentIds = null)
        {
            var column = SearchDropDownColumn(
                context: context,
                ss: ss,
                controlId: controlId,
                referenceId: referenceId,
                searchText: searchText,
                parentClass: parentClass,
                parentIds: parentIds,
                searchColumnOnly: false,
                searchFormat: false);
            if (isInitDisplay)
            {
                selectedValue.Deserialize<string[]>().ForEach(o =>
                {
                    if (!column.ChoiceHash?.ContainsKey(o) == true)
                    {
                        column.AddToChoiceHash(
                            context: context,
                            value: o);
                    }
                });
            }
            Dictionary<string, ControlData> optionCollection
                = new Dictionary<string, ControlData>();
            var multiple = (column.MultipleSelections ?? false) || filter;
            if (filter || parentIds?.Any() == true)
            {
                optionCollection = column?.EditChoices(
                    context: context,
                    addNotSet: filter,
                    own: filter);
            }
            if (parentIds?.Any() != true)
            {
                selectedValue = null;
            }
            else if (!multiple)
            {
                selectedValue = selectedValue.Deserialize<string[]>()?.FirstOrDefault();
            }
            return new ResponseCollection(context: context)
                .Html(
                    "#" + controlId,
                    new HtmlBuilder().OptionCollection(
                        context: context,
                        optionCollection: optionCollection,
                        selectedValue: selectedValue,
                        multiple: multiple,
                        addSelectedValue: false,
                        insertBlank: !filter,
                        column: column))
                .Invoke("callbackRelatingColumn", "#" + controlId)
                .ClearFormData("#" + controlId)
                .ToJson();
        }

        public static string SelectSearchDropDown(
            Context context,
            SiteSettings ss)
        {
            var controlId = context.Forms.Data("DropDownSearchTarget");
            var referenceId = context.Forms.Long("DropDownSearchReferenceId");
            var filter = controlId.StartsWith("ViewFilters__")
                || controlId.StartsWith("ProcessViewFilters__")
                || controlId.StartsWith("StatusControlViewFilters__")
                || controlId.StartsWith("DashboardViewFilters__")
                || controlId.StartsWith("ViewFiltersOnGridHeader__");
            var multiple = context.Forms.Bool("DropDownSearchMultiple");
            var selected = multiple
                ? context.Forms.List("DropDownSearchResultsAll")
                : context.Forms.List("DropDownSearchResults");
            var column = SearchDropDownColumn(
                context: context,
                ss: ss,
                controlId: controlId,
                referenceId: referenceId,
                selectedValues: selected,
                searchFormat: false);
            if (multiple)
            {
                return SelectSearchDropDownResponse(
                    context: context,
                    controlId: controlId,
                    column: column,
                    selected: selected,
                    filter: filter,
                    multiple: multiple);
            }
            else if (selected.Count() != 1)
            {
                return new ResponseCollection(context: context)
                    .Message(Messages.SelectOne(context: context))
                    .ToJson();
            }
            else
            {
                switch (controlId)
                {
                    case "InheritPermission":
                        return SelectSearchInheritPermissionDropDownResponse(
                            context: context,
                            ss: ss,
                            controlId: controlId,
                            selected: selected,
                            filter: filter,
                            multiple: multiple);
                    default:
                        return SelectSearchDropDownResponse(
                            context: context,
                            controlId: controlId,
                            column: column,
                            selected: selected,
                            filter: filter,
                            multiple: multiple);
                }
            }
        }

        private static Column SearchDropDownColumn(
            Context context,
            SiteSettings ss,
            string controlId,
            long referenceId,
            string searchText = "",
            List<string> selectedValues = null,
            int offset = 0,
            string parentClass = "",
            List<long> parentIds = null,
            bool searchColumnOnly = true,
            bool searchFormat = true)
        {
            var columnName = controlId.Replace("__", "_").Split_2nd('_');
            var column = ss.GetColumn(
                context: context,
                columnName: columnName);
            var searchIndexes = searchText.SearchIndexes();
            var link = column?.SiteSettings?.Links
                ?.Where(o => o.JsonFormat == true)
                .FirstOrDefault(o => o.ColumnName == column.Name);
            if (link != null)
            {
                if ((context.Forms.Bool("IsNew")
                    || ss.SiteId != referenceId)
                        && link.View?.ColumnFilterExpressions?.Any() == true)
                {
                    ItemUtilities.SetChoiceHashByFilterExpressions(
                        context: context,
                        ss: ss,
                        column: column,
                        referenceId: referenceId,
                        searchText: searchText,
                        offset: offset,
                        search: true,
                        searchFormat: searchFormat);
                }
                else
                {
                    var currentSs = ss.Destinations?.ContainsKey(link.SiteId) == true
                        ? ss
                        : column.SiteSettings;
                    column.SetChoiceHash(
                        context: context,
                        ss: currentSs,
                        link: link,
                        searchText: searchText,
                        parentColumn: currentSs.GetColumn(
                            context: context,
                            columnName: parentClass),
                        parentIds: parentIds,
                        offset: offset,
                        search: true,
                        searchFormat: searchFormat);
                }
            }
            else if (column?.Linked(context: context) == true)
            {
                column?.SetChoiceHash(
                    context: context,
                    siteId: column.SiteId,
                    linkHash: column.SiteSettings.LinkHash(
                        context: context,
                        columnName: column.Name,
                        searchIndexes: searchIndexes,
                        searchColumnOnly: searchColumnOnly,
                        selectedValues: selectedValues,
                        offset: offset,
                        parentClass: parentClass,
                        parentIds: parentIds,
                        setTotalCount: true),
                    searchIndexes: searchIndexes);
            }
            else
            {
                ss.SetChoiceHash(
                    context: context,
                    columnName: column?.ColumnName,
                    searchIndexes: searchIndexes,
                    setTotalCount: true);
            }
            return column;
        }

        private static string SelectSearchDropDownResponse(
            Context context,
            string controlId,
            Column column,
            List<string> selected,
            bool filter,
            bool multiple)
        {
            if (selected.Any()
                && column.UseSearch == true
                && column.Type != Column.Types.Normal
                && !selected.All(o => column.ChoiceHash.ContainsKey(o)))
            {
                switch (column.Type)
                {
                    case Column.Types.User:
                        selected
                            .Select(userId => SiteInfo.User(
                                context: context,
                                userId: userId.ToInt()))
                            .Where(o => !o.Anonymous())
                            .ForEach(user =>
                                column.ChoiceHash.AddIfNotConainsKey(
                                    user.Id.ToString(),
                                    new Choice(
                                        value: user.Id.ToString(),
                                        text: user.Name)));
                        break;
                    default:
                        selected
                            .Select(id =>
                            new
                            {
                                Id = id.ToInt(),
                                Name = SiteInfo.Name(
                                    context: context,
                                    id: id.ToInt(),
                                    type: column.Type)
                            })
                            .Where(o => o.Id != 0 && !o.Name.IsNullOrEmpty())
                            .ForEach(data =>
                                column.ChoiceHash.AddIfNotConainsKey(
                                    data.Id.ToString(),
                                    new Choice(
                                        value: data.Id.ToString(),
                                        text: data.Name)));
                        break;
                }
            }
            if (selected.Any() &&
                !selected
                    .Where(o => !(column.Type == Column.Types.User && o == "Own"))
                    .All(o => column.ChoiceHash.ContainsKey(o)))
            {
                column.SiteSettings.SetChoiceHash(
                    context: context,
                    columnName: column.ColumnName,
                    selectedValues: selected);
            }
            var optionCollection = column?.EditChoices(
                context: context,
                addNotSet: true,
                own: filter)
                    ?.Where(o => selected.Contains(o.Key))
                    .ToDictionary(o => o.Key, o => o.Value);
            return optionCollection?.Any() == true || !selected.Any()
                ? new ResponseCollection(context: context)
                    .CloseDialog("#DropDownSearchDialog")
                    .Html("[id=\"" + controlId + "\"]", new HtmlBuilder()
                        .OptionCollection(
                            context: context,
                            optionCollection: optionCollection,
                            selectedValue: SelectSearchDropDownSelectedValue(
                                context: context,
                                selected: selected,
                                filter: filter,
                                multiple: multiple),
                            multiple: multiple,
                            insertBlank: !filter))
                    .Invoke("setDropDownSearch")
                    .Trigger("#" + controlId, "change")
                    .ToJson()
                : new ResponseCollection(context: context)
                    .Message(Messages.NotFound(context: context))
                    .ToJson();
        }

        private static string SelectSearchInheritPermissionDropDownResponse(
            Context context,
            SiteSettings ss,
            string controlId,
            List<string> selected,
            bool filter,
            bool multiple)
        {
            var pastPermission = context.Forms.Data("InheritPermission");
            if (pastPermission.IsNullOrEmpty())
            {
                pastPermission = ss.InheritPermission.ToString();
            }
            var selectedPermission = selected.FirstOrDefault();
            var optionCollection = PermissionUtilities.InheritTargets(
                context: context,
                ss: ss).OptionCollection;
            return optionCollection?.Any() == true || !selected.Any()
                ? new ResponseCollection(context: context)
                    .CloseDialog("#DropDownSearchDialog")
                    .Html("[id=\"" + controlId + "\"]", new HtmlBuilder()
                        .OptionCollection(
                            context: context,
                            optionCollection: optionCollection,
                            selectedValue: SelectSearchDropDownSelectedValue(
                                context: context,
                                selected: selected,
                                filter: filter,
                                multiple: multiple),
                            multiple: multiple,
                            insertBlank: !filter))
                    .Invoke(
                        methodName: "setDropDownSearch",
                        _using: pastPermission != selectedPermission)
                    .ToJson()
                : new ResponseCollection(context: context)
                    .Message(Messages.NotFound(context: context))
                    .ToJson();
        }

        public static string SelectSearchDropDownSelectedValue(
            Context context, List<string> selected, bool filter, bool multiple)
        {
            if (multiple)
            {
                return selected.ToJson();
            }
            else
            {
                var value = selected.FirstOrDefault();
                return !filter && value == "\t"
                    ? null
                    : selected.FirstOrDefault();
            }
        }
    }
}