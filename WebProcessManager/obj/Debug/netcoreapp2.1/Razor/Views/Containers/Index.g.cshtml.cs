#pragma checksum "D:\Projects\ProcessManager\WebProcessManager\Views\Containers\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "f8a053d1d8f5040e4cef5932e6f4b5deb6850382"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Containers_Index), @"mvc.1.0.view", @"/Views/Containers/Index.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Containers/Index.cshtml", typeof(AspNetCore.Views_Containers_Index))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "D:\Projects\ProcessManager\WebProcessManager\Views\_ViewImports.cshtml"
using WebProcessManager;

#line default
#line hidden
#line 2 "D:\Projects\ProcessManager\WebProcessManager\Views\_ViewImports.cshtml"
using WebProcessManager.Models;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"f8a053d1d8f5040e4cef5932e6f4b5deb6850382", @"/Views/Containers/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"3d7e11a5e18c91c05c037991f93ad10c1867afd7", @"/Views/_ViewImports.cshtml")]
    public class Views_Containers_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<IEnumerable<WebProcessManager.Models.Container>>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("btn btn-primary btn-sm"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-action", "Create", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(56, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 3 "D:\Projects\ProcessManager\WebProcessManager\Views\Containers\Index.cshtml"
  
    ViewData["Title"] = "Index";
    ViewData["error"] = "";
    Layout = "~/Views/Shared/_Layout.cshtml";

#line default
#line hidden
            BeginContext(175, 36, true);
            WriteLiteral("\r\n<h2>Containers</h2>\r\n\r\n<div>\r\n    ");
            EndContext();
            BeginContext(211, 108, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("a", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "7c25cd874ffd4f14ad9855ccbac52022", async() => {
                BeginContext(265, 50, true);
                WriteLiteral("<span class=\"fa fa-plus-circle\"></span> Create New");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.Action = (string)__tagHelperAttribute_1.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_1);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(319, 44, true);
            WriteLiteral("&nbsp;\r\n</div>\r\n<p style=\"color: red\">\r\n    ");
            EndContext();
            BeginContext(364, 17, false);
#line 15 "D:\Projects\ProcessManager\WebProcessManager\Views\Containers\Index.cshtml"
Write(ViewData["error"]);

#line default
#line hidden
            EndContext();
            BeginContext(381, 3145, true);
            WriteLiteral(@"
</p>
<div style=""overflow: auto"">
    <div id=""gridContainer""></div>
</div>
<script>
    $(function () {
        var orders = new DevExpress.data.CustomStore({
            load: function (loadOptions) {
                var deferred = $.Deferred(),
                    args = {};

                if (loadOptions.sort) {
                    args.orderby = loadOptions.sort[0].selector;
                    if (loadOptions.sort[0].desc)
                        args.orderby += "" desc"";
                }

                args.skip = loadOptions.skip || 0;
                args.take = loadOptions.take || 12;

                $.ajax({
                    url: ""Containers/GetContainers"",
                    dataType: ""json"",
                    data: args,
                    success: function (result) {
                        console.log(result);
                        deferred.resolve(result.items, { totalCount: result.totalCount });
                    },
                    error: func");
            WriteLiteral(@"tion () {
                        deferred.reject(""Data Loading Error"");
                    },
                    timeout: 5000
                });

                return deferred.promise();
            }
        });

            $(""#gridContainer"").dxDataGrid({
                dataSource: {
                    store: orders
                },
                showColumnLines: true,
                showRowLines: true,
                rowAlternationEnabled: false,
                showBorders: true,
                columnAutoWidth: true,
                allowColumnResizing: true,
                columnResizingMode: 'widget',
                searchPanel: {
                    visible: true,
                    width: 240,
                    placeholder: ""Search...""
                },
                headerFilter: {
                    visible: true
                },
                remoteOperations: {
                    sorting: true,
                    paging: true
        ");
            WriteLiteral(@"        },
                paging: {
                    pageSize: 12
                },
                pager: {
                    showPageSizeSelector: true,
                    allowedPageSizes: [8, 12, 20]
                },
                columns: [
                    {
                        dataField: ""id"",
                        headerCellTemplate: $('<p style=""color: black""><strong>Id</strong></p>'),
                        width: 50,
                        allowResizing: false
                    },
                    {
                        width: 60,
                        allowFiltering: false,
                        allowSorting: false,
                        allowResizing: false,
                        cellTemplate: function(container, options) {
                            container.append(""<a id='link"" + options.data.id + ""' >Detail</a>"");
                            $(""#link"" + options.data.id).click(function() {
                                var id = o");
            WriteLiteral("ptions.data.id;\r\n                                window.location.href = \'");
            EndContext();
            BeginContext(3527, 35, false);
#line 100 "D:\Projects\ProcessManager\WebProcessManager\Views\Containers\Index.cshtml"
                                                   Write(Url.Action("Details", "Containers"));

#line default
#line hidden
            EndContext();
            BeginContext(3562, 1306, true);
            WriteLiteral(@"/' + id;
                            }).css({cursor:""pointer""});
                        }
                    },
                    {
                        dataField: ""name"",
                        headerCellTemplate: $('<p style=""color: black""><strong>Name</strong></p>')
                    },
                    {
                        dataField: ""address"",
                        headerCellTemplate: $('<p style=""color: black""><strong>Url Address</strong></p>')
                    },
                    {
                        allowFiltering: false,
                        allowSorting: false,
                        cellTemplate: function (container, options) {
                            container
                                .append(""<a class='btn btn-sm btn-default btnInGrid' id='btnEdit"" + options.data.id + ""' role='button'><span class='fa fa-edit'></span> Edit</a> "")
                                .append(""<a class='btn btn-sm btn-default btnInGrid' id='btnDelete"" + opti");
            WriteLiteral(@"ons.data.id + ""' role='button'><span class='fa fa-trash-alt'></span> Delete</a> "");
                            $(""#btnDelete"" + options.data.id).click(function() {
                                var id = options.data.id;
                                window.location.href = '");
            EndContext();
            BeginContext(4869, 34, false);
#line 121 "D:\Projects\ProcessManager\WebProcessManager\Views\Containers\Index.cshtml"
                                                   Write(Url.Action("Delete", "Containers"));

#line default
#line hidden
            EndContext();
            BeginContext(4903, 238, true);
            WriteLiteral("/\' + id;\r\n                            });\r\n                            $(\"#btnEdit\" + options.data.id).click(function() {\r\n                                var id = options.data.id;\r\n                                window.location.href = \'");
            EndContext();
            BeginContext(5142, 32, false);
#line 125 "D:\Projects\ProcessManager\WebProcessManager\Views\Containers\Index.cshtml"
                                                   Write(Url.Action("Edit", "Containers"));

#line default
#line hidden
            EndContext();
            BeginContext(5174, 168, true);
            WriteLiteral("/\' + id;\r\n                            });\r\n                        }\r\n                    }\r\n                ]\r\n        }).dxDataGrid(\"instance\");\r\n    });\r\n</script>\r\n");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<IEnumerable<WebProcessManager.Models.Container>> Html { get; private set; }
    }
}
#pragma warning restore 1591
