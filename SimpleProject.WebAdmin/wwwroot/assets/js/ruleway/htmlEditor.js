var htmlEditorClass = function (el, options) {
    if ($(el).data("htmlEditor")) {
        return $(el).data("htmlEditor");
    }

    var element = $(el);

    function init() {
        var settings = $.extend({}, {
            height: 500,
            disableDragAndDrop: true,
            lang: $lang.code,
            callbacks: {
                onImageUpload: function (files, editor, editable) {

                    $app.showMask("body");

                    var data = new FormData();
                    data.append("file", files[0]);

                    $.ajax({
                        url: "/home/saveimage",
                        data: data,
                        cache: false,
                        contentType: false,
                        processData: false,
                        type: 'POST',
                        success: function (result) {
                            $app.hideMask("body");

                            if (result.redirect) {
                                location.href = result.redirect;
                                return;
                            }

                            if (result.hasError !== undefined) {
                                if (result.hasError) {
                                    if (result.errors && result.errors.length) {
                                        $app.toast("danger", $app.resultError(result));
                                    }
                                    return;
                                }
                            }

                            if (result.data) {
                                element.summernote("pasteHTML", '<img src="' + result.data + '" class="img-fluid"/>');
                            }
                        },
                        error: function (xhr, status, error) {
                            $app.hideMask("body");
                            $app.toast("error", $lang.errorOccurred);
                        }
                    });
                },
                onInit: function () {
                    $("button[data-toggle='dropdown']").each(function () {
                        $(this).removeAttr("data-toggle").attr("data-bs-toggle", "dropdown");
                    });
                }
            }
        }, options);

        element.summernote(settings);
    }

    this.getCode = function () {
        return element.summernote('code');
    }

    this.encode = function(html) {
        html = replaceAll("=", "~", html);
        html = replaceAll("&", "^", html);
        return html;
    }

    init();
}

function $htmlEditor(el) {
    var editor = $(el).data("htmlEditor");
    if (editor) {
        return editor;
    }
    var options = {};
    if ($(el).attr("data-height")) 
    {
        options.height = $(el).attr("data-height");
    }
    return new htmlEditorClass(el, options);
}

$(function () {
    $(".html-editor").each(function () {
        $htmlEditor(this);
    });
    $app.ajaxComplete.push(function () {
        $(".html-editor").each(function () {
            $htmlEditor(this);
        });
    });
});