var gridClass = function (el) {
    if ($(el).data("grid")) {
        return $(el).data("grid");
    }

    var element = $(el), $this = this, prevJx, fields = [], events = [];
    var columns = element.find("thead > tr > th");
    var body = element.find("tbody");

    var gridFooter, pagination, info, pagesize;
    var readUrl = element.attr("data-url");
    var initalHeightSet = false;

    element.attr("data-total-record", 0);
    this.load = function (reset) {
        if (prevJx) {
            prevJx.abort();
        }

        var req = {
            Page: 1,
            PageSize: 20,
            Sorting: null,
            Filters: [],
            SessionKey: element.attr("data-grid-id"),
            Fields: fields
        };

        if (reset) {
            element.attr("data-page", "1");
        }

        if (element.find("thead > tr > th.sorting_asc").length) {
            req.Sorting = element.find("thead > tr > th.sorting_asc").attr("data-field") + ":ASC";
        }
        else if (element.find("thead > tr > th.sorting_desc").length) {
            req.Sorting = element.find("thead > tr > th.sorting_desc:first").attr("data-field") + ":DESC";
        }

        if (element.attr("data-page-size")) {
            req.PageSize = parseInt(element.attr("data-page-size"));
        }
        if (element.attr("data-page")) {
            req.Page = parseInt(element.attr("data-page"));
        }

        req.Filters = $this.getFilters();

        callEvent("beforeLoad");

        prevJx = $app.callJx(readUrl, element, req, function (result) {
            if (!result || typeof result !== "object") {
                $app.toast("danger", $lang.errorOccurred);
                return;
            }

            callEvent("preRender");

            setPage(result);

            body.empty();

            if (!initalHeightSet){
                initalHeightSet = true;
                $this.setAutoHeight();
            }

            if (result.total === 0) {
                callEvent("nodataLoad");
                body.append($("<tr>").append($("<td>", { colspan: columns.length, text: $lang.recordNotFound })));
                return;
            }

            $.each(result.data, function (i, e) {
                var row = $("<tr>");
                $.each(columns, function (j, col) {
                    var column = $(col);
                    var td = $("<td>", { html: e[j] });
                    if (column.attr("data-template")) {
                        var fn = $template.getTemplateFn(column.attr("data-template"));
                        if (fn) {
                            td.html(fn.call($this, e, j, td, row));
                        }
                    }
                    if (column.attr("data-hidden") == "1") {
                        td.addClass("d-none");
                    }
                    if (!$app.isEmpty(column.attr("data-css"))) {
                        td.addClass(column.attr("data-css"));
                    }
                    row.append(td);
                });
                body.append(row);
            });


            if (element.attr("data-filter")) {
                var filter = element.attr("data-filter");
                $(filter).find(":input").each(function () {
                    var input = $(this);
                    var val = input.val();

                    input.attr("title", val);
                });
            }

            element.parents(".table-responsive:first").scrollTop(0);

            callEvent("load");
            $(window).trigger("resize");
        });
    }

    this.reload = function () {
        $this.load(true);
    }

    this.getFilters = function () {
        var filters = [];
        if (element.attr("data-filter")) {
            var filter = element.attr("data-filter");
            $(":input[data-field]", filter).each(function (i, e) {
                var item = $(e);
                var value = item.val();
                if (value) {
                    filters.push({
                        Field: item.attr("data-field"),
                        Operant: item.attr("data-operant") ? item.attr("data-operant") : "*",
                        Value: value
                    });
                }
            });
        }
        return filters;
    }

    this.download = function (format, sample, options) {
        if (!$("#excelForm").length) {
            $("body").append('<form id="excelForm" method="post" action=""></form>');
        }

        if ($app.isEmpty(format)) {
            format = "excel";
        }
        $("#excelForm").empty();

        var filters = $this.getFilters();

        if (filters) {
            $.each(filters, function (i, e) {
                $("#excelForm").append($("<input>", { type: "hidden", name: "Filters[" + i + "].Field", value: e.Field }));
                $("#excelForm").append($("<input>", { type: "hidden", name: "Filters[" + i + "].Operant", value: e.Operant }));
                $("#excelForm").append($("<input>", { type: "hidden", name: "Filters[" + i + "].Value", value: e.Value }));
            });
        }

        if (element.find("thead > tr > th.sorting_asc").length) {
            $("#excelForm").append($("<input>", { type: "hidden", name: "Sorting", value: (element.find("thead > tr > th.sorting_asc").attr("data-field") + ":ASC") }));
        }
        else if (element.find("thead > tr > th.sorting_desc").length) {
            $("#excelForm").append($("<input>", { type: "hidden", name: "Sorting", value: (element.find("thead > tr > th.sorting_desc:first").attr("data-field") + ":DESC") }));
        }

        $("#excelForm").append($("<input>", { type: "hidden", name: "Page", value: "1" }));
        $("#excelForm").append($("<input>", { type: "hidden", name: "PageSize", value: "0" }));
        $("#excelForm").append($("<input>", { type: "hidden", name: "format", value: format }));
        $("#excelForm").append($("<input>", { type: "hidden", name: "sample", value: (sample ? "true" : "false") }));
        if (options && $.isPlainObject(options)) {
            $.each(options, function (key, val) {
                $("#excelForm").append($("<input>", { type: "hidden", name: key, value: val }));
            });
        }

        $("#excelForm").attr('action', readUrl);
        $("#excelForm").trigger("submit");
    };

    this.getElement = function () {
        return element;
    }

    this.id = function () {
        return element.attr("id");
    }

    this.on = function (name, fn) {
        if (!name || name == "") {
            return;
        }
        if (typeof fn != "function") {
            return;
        }
        events.push({ name: name, fn: fn });
    }

    this.readUrl = function (newUrl) {
        if (newUrl) {
            readUrl = newUrl;
        }

        return readUrl;
    }

    this.setAutoHeight = function(){
        element.find("tbody").addClass("d-none");
        var maxHeight = $(window).height();
        element.parents(".table-responsive:first").css("max-height", "");

        if (element.parents(".modal").length){
            var modal = element.parents(".modal:first");
            maxHeight = modal.innerHeight() - (modal.find(".modal-dialog").outerHeight(true) - modal.find(".modal-dialog").innerHeight())
                - (modal.find(".modal-dialog").outerHeight(true) - modal.find(".modal-dialog").innerHeight())
                - (modal.find(".modal-content").outerHeight(true) - modal.find(".modal-content").innerHeight())
                - (modal.find(".modal-body").outerHeight(true) - modal.find(".modal-body").innerHeight())
                - (modal.find(".modal-header").outerHeight(true) ?? 0)
                - (modal.find(".modal-footer").outerHeight(true) ?? 0);
        }
        else{
            maxHeight = window.innerHeight -  ($(".main-content").outerHeight(true) - $(".main-content").innerHeight()) - $(".footer").outerHeight() - $(".app-header").outerHeight();
        }

        var card = element.parents(".card:first");
        card.parent().children().each(function(){
            if (this != card.get(0)){
                maxHeight -= ($(this).outerHeight(true) ?? 0);
            }
        });
        
        element.parents(".table-responsive:first").css("max-height", maxHeight);

        element.find("tbody").removeClass("d-none");
    }

    this.getFieldIndex = function(field){
        var index = -1;
        $.each(columns, function (i, e) {
            if (index >= 0){
                return;
            }
            var column = $(e);
            if (column.attr("data-field") == field) {
                index = i;
            }
        });
        return index;
    }

    var callEvent = function (name) {
        if (events) {
            for (var i = 0; i < events.length; i++) {
                if (events[i].name == name) {
                    events[i].fn.call($this);
                }
            }
        }
    }

    this.scrollTop = function (top) {
        if (top === undefined) {
            return element.parents(".table-responsive:first").scrollTop();
        }
        setTimeout(function () {
            element.parents(".table-responsive:first").scrollTop(top);
        }, 100);
    }

    var setPage = function (result) {
        var page = parseInt(element.attr("data-page"));
        var totalPages = result.hasError ? 1 : Math.ceil(result.total / parseInt(element.attr("data-page-size")));
        var total = result.hasError ? 0 : result.total;
        var adjacents = 3;

        element.attr("data-total-record", total);

        pagination.empty();
        pagination.append(
            $("<li>", { class: "page-item " + (page > 1 ? "" : " disabled") }).append(
                $("<a>", { class: "page-link", "data-page": (page - 1), href: "javascript:;", html: $lang.back })
            )
        );
        if (totalPages > 1) {
            if (totalPages < 7 + (adjacents * 2)) { //not enough pages to bother breaking it up
                for (var i = 1; i <= totalPages; i++) {
                    pagination.append(
                        $("<li>", { class: "page-item" + (i == page ? " active" : "") }).append(
                            $("<a>", { class: "page-link", "data-page": i, href: "javascript:;", html: i })
                        )
                    );
                }
            }
            else if (totalPages > 5 + (adjacents * 2)) { //enough pages to hide some
                if (page < 1 + (adjacents * 2)) { //close to beginning; only hide later pages
                    for (var i = 1; i < 4 + (adjacents * 2); i++) {
                        pagination.append(
                            $("<li>", { class: "page-item" + (i == page ? " active" : "") }).append(
                                $("<a>", { class: "page-link", "data-page": i, href: "javascript:;", html: i })
                            )
                        );
                    }
                    pagination.append(
                        $("<li>", { class: "page-item" }).append(
                            $("<a>", { class: "page-link", href: "javascript:;", html: "..." })
                        )
                    );
                    pagination.append(
                        $("<li>", { class: "page-item" }).append(
                            $("<a>", { class: "page-link", "data-page": totalPages, href: "javascript:;", html: totalPages })
                        )
                    );
                }
                else if (totalPages - (adjacents * 2) > page && page > (adjacents * 2)) { //in middle; hide some front and some back
                    pagination.append(
                        $("<li>", { class: "page-item" }).append(
                            $("<a>", { class: "page-link", "data-page": "1", href: "javascript:;", html: "1" })
                        )
                    );

                    pagination.append(
                        $("<li>", { class: "page-item" }).append(
                            $("<a>", { class: "page-link", href: "javascript:;", html: "..." })
                        )
                    );

                    for (var i = page - adjacents; i <= page + adjacents; i++) {
                        pagination.append(
                            $("<li>", { class: "page-item" + (i == page ? " active" : "") }).append(
                                $("<a>", { class: "page-link", "data-page": i, href: "javascript:;", html: i })
                            )
                        );
                    }

                    pagination.append(
                        $("<li>", { class: "page-item" }).append(
                            $("<a>", { class: "page-link", href: "javascript:;", html: "..." })
                        )
                    );
                    pagination.append(
                        $("<li>", { class: "page-item" }).append(
                            $("<a>", { class: "page-link", "data-page": totalPages, href: "javascript:;", html: totalPages })
                        )
                    );
                }
                else { //close to end; only hide early pages
                    pagination.append(
                        $("<li>", { class: "page-item" }).append(
                            $("<a>", { class: "page-link", "data-page": "1", href: "javascript:;", html: "1" })
                        )
                    );

                    pagination.append(
                        $("<li>", { class: "page-item" }).append(
                            $("<a>", { class: "page-link", href: "javascript:;", html: "..." })
                        )
                    );

                    for (var i = totalPages - (2 + (adjacents * 2)); i <= totalPages; i++) {
                        pagination.append(
                            $("<li>", { class: "page-item" + (i == page ? " active" : "") }).append(
                                $("<a>", { class: "page-link", "data-page": i, href: "javascript:;", html: i })
                            )
                        );
                    }
                }
            }
        }
        else {
            pagination.append(
                $("<li>", { class: "page-item disabled" }).append(
                    $("<a>", { class: "page-link", href: "javascript:;", html: "1" })
                )
            );
        }

        pagination.append(
            $("<li>", { class: "page-item " + (page < totalPages ? "" : " disabled") }).append(
                $("<a>", { class: "page-link", "data-page": (page + 1), href: "javascript:;", html: $lang.next })
            )
        );

        if (!info.find(".total-count").length) {
            info.append(
                $("<div>", { class: "mt-2 mx-3", html: $app.format($lang.recordTotals, '<strong class="total-count"></strong> ') })
            );
            pagesize.append(
                $("<div>", { class: "d-flex flex-row" }).append(
                    $("<div>", { class: "mt-2 me-1", html: $lang.pageSize + ": " }),
                    $("<div>", { style: "width: 5rem" }).append(
                        $("<select>", { class: "select2", "data-minimum-results-for-search": "Infinity" }).append(
                            //'<option value="20">20</option>',
                            '<option value="50">50</option>',
                            '<option value="100">100</option>',
                            '<option value="1000">1000</option>'
                        )
                    )
                )
            );
            pagesize.find("select").val(element.attr("data-page-size"));
            pagesize.find("select").change(function () {
                element.attr("data-page", "1");
                element.attr("data-page-size", $(this).val());
                $this.load();
            }).val(element.attr("data-page-size"));
        }

        info.find(".total-count").html(total);
    }

    var clearColumnSorting = function () {
        $.each(columns, function (i, e) {
            var column = $(e);
            if (column.hasClass("sorting_desc") || column.hasClass("sorting_asc") || column.hasClass("sorting")) {
                column.removeClass("sorting_asc").removeClass("sorting_desc").addClass("sorting");
            }
        });
    }

    var applyFilterCss = function (e) {
        var item = $(e);
        if (item.hasClass("select2")) {
            if (item.val()) {
                item.next().find(".selection > .select2-selection").addClass("text-primary").addClass("border-primary");
            }
            else {
                item.next().find(".selection > .select2-selection").removeClass("text-primary").removeClass("border-primary");
            }
        }
        else {
            if (item.val()) {
                item.addClass("text-primary").addClass("border-primary");
            }
            else {
                item.removeClass("text-primary").removeClass("border-primary");
            }
        }
    }

    element.data("grid", this);

    gridFooter = $("<div>", { class: "row" }).append(
        $("<div>", { class: "col-12" }).append(
            $("<div>", { class: "d-flex justify-content-between flex-wrap overflow-auto" }).append(
                $("<div>", { class: "mb-2 mb-sm-0 grid-info" }),
                $("<ul>", { class: "pagination mb-0 me-4 overflow-auto grid-pager" }),
                $("<div>", { class: "mb-2 mb-sm-0 grid-page-size" })
            )
        )
    );
    gridFooter.insertAfter(element.parents(".card:first"));

    pagination = gridFooter.find(".grid-pager");
    info = gridFooter.find(".grid-info");
    pagesize = gridFooter.find(".grid-page-size");

    body.append($("<tr>").append($("<td>", { colspan: columns.length, text: $lang.recordNotFound })));
    columns.each(function () {
        var column = $(this);
        if (column.attr("data-orderable") == "1") {
            column.addClass("sorting");
        }
        if (!$app.isEmpty(column.attr("data-order"))) {
            column.addClass(column.attr("data-order").toLowerCase() == "asc" ? "sorting_asc" : "sorting_desc");
        }
        if (!$app.isEmpty(column.attr("data-field"))) {
            fields.push(column.attr("data-field"));
        }
    });

    element.on("click", ".sorting,.sorting_desc,.sorting_asc", function () {
        var item = $(this);
        if (item.hasClass("sorting_desc")) {
            clearColumnSorting();
        }
        else if (item.hasClass("sorting_asc")) {
            clearColumnSorting();
            item.removeClass("sorting").removeClass("sorting_asc").addClass("sorting_desc");
        }
        else if (item.hasClass("sorting")) {
            clearColumnSorting();
            item.removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
        }
        $this.load();
    });

    pagination.on("click", "[data-page]", function () {
        var item = $(this);
        element.attr("data-page", item.attr("data-page"));
        $this.load();
    });

    if (element.attr("data-filter")) {
        var filter = element.attr("data-filter");
        $(filter).find("select, input").on("change", function () {
            applyFilterCss(this);
            $this.reload();
        });
        $(filter).find(".search-btn,:input[type='submit']").on("click", function (e) {
            $this.reload();
        });
        $(filter).find("select, input").each(function () {
            applyFilterCss(this);
        });
    }

    element.on("click", ".grid-edit", function () {
        var row = $(this).parents("tr:first");
        if (typeof window["editRecord"] == "function") {
            window["editRecord"].call(this, $this.id());
        }
    });

    element.on("click", ".grid-delete", function () {
        if (typeof window["deleteRecord"] == "function") {
            window["deleteRecord"].call(this, $this.id());
        }
    });

    var loadData = true;
    if (element.is("[data-prevent-load]")) {
        loadData = element.attr("data-prevent-load") != "1";
    }
    if (loadData) {
        $this.load();
    }

    $(window).on("resize", element, function () {
        $this.setAutoHeight();
    });
};

function $grid(el) {
    var grd = $(el).data("grid");
    if (grd) {
        return grd;
    }
    return new gridClass(el);
}

$(function () {
    $(".ruleway-grid").each(function () {
        $grid(this);
    });
    $app.ajaxComplete.push(function () {
        $(".ruleway-grid").each(function () {
            $grid(this);
        });
    });
});