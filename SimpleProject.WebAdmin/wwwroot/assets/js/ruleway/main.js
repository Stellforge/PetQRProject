var $app = function () {

    function ajaxDone () {
        $.each($app.ajaxComplete, function () {
            var fn = this;
            if (typeof fn == "function") {
                fn();
            }
        });
    }

    return {
        hideMask: function (block) {
            if (block) {
                $(block).unblock();
            }
            else {
                $.unblockUI();
            }
        },
        showMask: function (block, msg) {
            if (block) {
                $(block).block({
                    message: '<span class="spinner-border" style="width: 3rem; height: 3rem;"></span>' + (msg != undefined ? msg : ""),
                    overlayCSS: {
                        backgroundColor: '#fff',
                        opacity: 0.8,
                        cursor: 'wait',
                        'box-shadow': '0 0 0 1px #ddd'
                    },
                    css: {
                        border: 0,
                        padding: 0,
                        backgroundColor: 'none'
                    }
                });
            }
            else {
                $.blockUI({
                    message: '<span class="spinner-border" style="width: 3rem; height: 3rem;"></span>' + (msg != undefined ? msg : ""),
                    overlayCSS: {
                        backgroundColor: '#fff',
                        opacity: 0.8,
                        cursor: 'wait',
                        'box-shadow': '0 0 0 1px #ddd'
                    },
                    css: {
                        border: 0,
                        padding: 0,
                        backgroundColor: 'none'
                    }
                });
            }
        },
        setCookie: function (cname, cvalue, exdays) {
            if (exdays)
                exdays = 1000;

            var d = new Date();
            d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
            var expires = "expires=" + d.toUTCString();
            document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
        },
        getCookie: function (cname) {
            var name = cname + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) == ' ') {
                    c = c.substring(1);
                }
                if (c.indexOf(name) == 0) {
                    return c.substring(name.length, c.length);
                }
            }
            return "";
        },
        showModal: function (type, title, body, fn, fnClose, fnShown) {
            var bgcss = type == "danger" ? "bg-danger" : "bg-primary";
            var modal = $("<div>", { class: "modal fade", tabindex: "-1" }).append(
                $("<div>", { class: "modal-dialog" }).append(
                    $("<div>", { class: "modal-content" }).append(
                        $("<div>", { class: "modal-header " + bgcss }).append(
                            $("<h6>", { class: "modal-title text-white", html: title }),
                            $("<button>", { type: "button", class: "btn-close btn-close-white", "data-bs-dismiss": "modal", html: "" }),
                        ),
                        $("<div>", { class: "modal-body" }).append(
                            body
                        ),
                        $("<div>", { class: "modal-footer" }).append(
                            $("<button>", { type: "button", class: "btn btn-dark cancel-btn", "data-bs-dismiss": "modal" }).append(
                                $("<i>", { class: "ri-close-line me-1" }),
                                $lang.close
                            ),
                            $("<button>", { type: "button", class: "btn ok-btn " + (type == "danger" ? "btn-danger" : "btn-primary") }).append(
                                $("<i>", { class: "ri-save-line me-1" }),
                                $lang.save
                            )
                        )
                    )
                )
            );
            if (!title) {
                modal.find(".modal-header").remove();
            }
            if (!fn) {
                modal.find(".ok-btn").remove();
            }

            modal.appendTo("body");

            if (fn && typeof fn == "function") {
                modal.find(".ok-btn").on("click", fn);
            }

            modal.on("hidden.bs.modal", function (e) {
                if (e.target === this) {
                    modal.remove();
                    if ($(".modal.show").length) {
                        $("body").addClass("modal-open");
                    }
                    if (fnClose && typeof fnClose == "function") {
                        fnClose();
                    }
                }
            });

            if (fnShown && typeof fnShown === "function") {
                modal.on("shown.bs.modal", function (e) {
                    fnShown(e);
                });
            }

            new bootstrap.Modal(modal.get(0), { backdrop: 'static' }).show();

            var zindex = 0;
            $(".modal").each(function (i, e) {
                var index = parseInt($(e).css("z-index"));
                if (index > zindex) {
                    zindex = index;
                }
            });
            if (zindex > 0) {
                var index = parseInt(zindex);
                $(".modal-backdrop:last").css("z-index", index + 1);
                modal.css("z-index", index + 2);
            }

            return modal;
        },
        alert: function (type, msg, fnClose) {
            var content = $("<div>", { class: "d-flex" }).append(
                $("<i>", { class: "ri-checkbox-circle-line h2 mb-0 me-2 text-success" }),
                $("<div>", { class: "align-self-center", html: msg })
            );
            if (type == "danger") {
                content.find("i").removeClass("ri-checkbox-circle-line").removeClass("text-success").addClass("text-danger ri-alert-line");
            }
            $app.showModal(type, undefined, content, undefined, fnClose);
        },
        toast: function (type, msg, fn, noTimeout) {
            var content = "<div class='d-flex'><i class='ri-checkbox-circle-line h2 mb-0 me-2'></i><div class='align-self-center'>" + msg + "</div></div>";   
            if (type == "danger") {
                content = "<div class='d-flex'><i class='ri-alert-line h2 mb-0 me-2'></i><div class='align-self-center'>" + msg + "</div></div>";
            }

            var opt = {
                text: content,
                position: "center",
                close: true,
                duration: -1,
                escapeMarkup: false,
                className: "alert bg-" + type
            }
            if (!noTimeout) {
                opt["duration"] = 5000;
            }
            Toastify(opt).showToast();
            if (fn && typeof fn === "function") {
                fn();
            }
        },
        confirm: function (msg, fn, type) {
            var content = $("<div>", { class: "d-flex" }).append(
                $("<i>", { class: "ri-question-line h2 mb-0 me-2 text-info" }),
                $("<div>", { class: "align-self-center", html: msg })
            );
            if (type == "danger") {
                content.find("i").removeClass("text-info").addClass("text-danger");
            }

            var confirmModal = $app.showModal(type, undefined, content, function () {
                fn();
                confirmModal.modal("hide");
            });
            confirmModal.find(".cancel-btn").empty().append(
                $("<i>", { class: "ri-close-line me-1" }),
                $lang.no
            );
            confirmModal.find(".ok-btn").empty().append(
                $("<i>", { class: "ri-check-line me-1" }),
                $lang.yes
            );
        },
        openLinkModal: function (url, data, fn, fnLoad, fnClose) {
            var modal = $app.showModal("success", undefined, "<div class='inner-content'></div>", fn, fnClose);
            //modal.find(".modal-dialog").addClass("modal-lg");

            modal.find(".modal-dialog").removeClass("modal-lg").addClass("modal-xl");

            $app.getJx(url, modal.find(".modal-content"), data, function (result) {
                modal.find(".modal-body").html(result);
                var suffix = "_" + Math.floor((Math.random() * 10000));
                modal.find(":input[id]").each(function () {
                    $(this).attr("id", $(this).attr("id") + suffix);
                });
                modal.find("label[for]").each(function () {
                    $(this).attr("for", $(this).attr("for") + suffix);
                });
                if (fnLoad && typeof fnLoad == "function") {
                    fnLoad();
                }
            }, function () {
                modal.find(".modal-body").append(
                    $("<div>", { class: "alert bg-danger text-white", html: $lang.errorOccurred })
                );
            });
            return modal;
        },
        showUpload: function (url, fn) {
            var form = $("<form>", { action: url, method: "post" }).append(
                $("<div>", { class: "row mb-3" }).append(
                    $("<label>", { class: "col-form-label col-lg-3", for: "excelInput", text: $lang.excelFile }),
                    $("<div>", { class: "col-lg-9" }).append(
                        $("<input>", { type: "file", class: "form-control", id: "excelInput", name: "file" })
                    )
                )
            );

            var modal = $app.showModal("success", undefined, form, function () {
                $app.postJx(url, "body", modal.find("form"), function (result) {
                    if (fn && typeof fn == "function") {
                        fn.call(modal, result);
                    }
                    if (!$app.isEmpty(result.data.filePath)) {
                        modal.find("form").find(".error-msg").remove();
                        modal.find("form").append(
                            $("<div>", { class: "row mb-3 error-msg" }).append(
                                $("<div>", { class: "col-lg-12" }).append(
                                    $("<p>", { class: "text-danger", html: $lang.uploadDoneSomeErrors }),
                                    $("<a>", { href: result.data.filePath, html: $lang.excelErrorFile })
                                )
                            )
                        );
                    }
                    else {
                        $app.toast("success", $lang.uploadDone, function () {
                            modal.modal("hide");
                        });
                    }
                });
            });
            modal.find(".ok-btn").empty().append(
                $("<i>", { class: "ri-file-upload-line me-1" }),
                $lang.upload
            );
            modal.find(".modal-dialog").addClass("modal-lg");
        },
        callJx: function (url, mask, data, done, fail) {
            $app.showMask(mask);

            var jx = $.ajax({
                type: 'POST',
                url: url,
                data: data,
                cache: false,
                success: function (result) {
                    $app.hideMask(mask);

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

                    if (done && typeof done === 'function') {
                        done.call(undefined, result);
                    }

                    if (result.warnings && result.warnings.length) {
                        $app.toast("warning", result.warnings.join("<br />"));
                    }

                    ajaxDone();
                },
                error: function (xhr, status, error) {
                    $app.hideMask(mask);
                    if (fail && typeof fail === 'function') {
                        fail.call(undefined);
                    }
                    else {
                        if (status != "abort") {
                            $app.toast("danger", $lang.errorOccurred);
                        }
                    }
                }
            });
            return jx;
        },
        getJx: function (url, mask, data, done, fail) {
            $app.showMask(mask);

            var jx = $.ajax({
                type: 'GET',
                url: url,
                data: data,
                cache: false,
                success: function (result) {
                    $app.hideMask(mask);

                    if (result && typeof result === 'object') {
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
                    }

                    if (done && typeof done === 'function') {
                        done.call(undefined, result);
                    }

                    if (result.warnings && result.warnings.length) {
                        $app.toast("warning", result.warnings.join("<br />"));
                    }

                    ajaxDone();
                },
                error: function (xhr, status, error) {
                    $app.hideMask(mask);
                    if (fail && typeof fail === 'function') {
                        fail.call(undefined);
                    }
                    else {
                        $app.toast("danger", $lang.errorOccurred);
                    }
                }
            });

            return jx;
        },
        postJx: function (url, mask, form, done, fail) {
            $app.showMask(mask);

            var data = new window.FormData($(form).get(0));

            var jx = $.ajax({
                type: 'POST',
                method: 'POST',
                data: data,
                cache: false,
                contentType: false,
                processData: false,
                url: url,
                success: function (result) {
                    $app.hideMask(mask);

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

                    if (done && typeof done === 'function') {
                        done.call(undefined, result);
                    }

                    if (result.warnings && result.warnings.length) {
                        $app.toast("warning", result.warnings.join("<br />"));
                    }

                    ajaxDone();
                },
                error: function (xhr, status, error) {
                    $app.hideMask(mask);
                    if (fail && typeof fail === 'function') {
                        fail.call(undefined);
                    }
                    else {
                        $app.toast("danger", $lang.errorOccurred);
                    }
                }
            });
            return jx;
        },
        resultError: function (result) {
            if (!result.errors) {
                return $lang.errorOccurred;
            }
            return result.errors.join("<br />");
        },
        isEmpty: function (input) {
            return input == undefined || input == null || input == "" || (typeof (input) == "object" && Object.keys(input).length == 0);
        },
        gotoUrl: function (url, data) {
            $app.showMask("body", $lang.redirecting);
            window.location.href = $app.getUrl(url, data);
        },
        getUrl: function (url, data) {
            var link = url;
            if (data) {
                var params = [];
                for (var key in data) {
                    params.push(encodeURIComponent(key) + "=" + encodeURIComponent(data[key]));
                }
                link += (link.indexOf("?") < 0 ? "?" : "&") + params.join("&");
            }
            return link;
        },
        getQueryParam: function (url, key) {
            const urlParams = url.indexOf("?") > 0 ? url.substring(url.indexOf("?") + 1) : "";
            const paramArr = urlParams.split('&');
            for (var param of paramArr) {
                var values = param.split('=');
                if (values.length == 2 && values[0] == key) {
                    return decodeURIComponent(values[1]);
                }
            }
        },
        changeQueryParam: function (url, key, value) {
            const link = url.indexOf("?") >= 0 ? url.substring(0, url.indexOf("?")) : url;
            const urlParams = url.indexOf("?") >= 0 ? url.substring(url.indexOf("?") + 1) : "";
            const paramArr = urlParams.split('&');

            var params = [];
            for (var param of paramArr) {
                var values = param.split('=');
                if (values.length == 2 && values[0] == key) {
                    continue;
                }
                params.push(param);
            }

            params.push(encodeURIComponent(key) + "=" + encodeURIComponent(value));

            return link + "?" + params.join("&");
        },
        format: function () {
            var formatted = arguments[0];
            for (var i = 1; i < arguments.length; i++) {
                var regexp = new RegExp('\\{' + (i - 1) + '\\}', 'gi');
                formatted = formatted.replace(regexp, arguments[i]);
            }
            return formatted;
        },
        triggerAjaxDone: function () {
            ajaxDone();
        },
        toNumber: function (val) {
            if ($app.isEmpty(val)) {
                return 0;
            }
            return parseFloat(val.replace($app.strToRegex($numberFormat.group), '').replace($app.strToRegex($numberFormat.decimal), '.'));
        },
        toNumberString: function (val, grouped) {
            if ($app.isEmpty(val)) {
                return 0;
            }
            if (typeof val === 'number') {
                let number = val.toLocaleString('en-US');
                if (grouped) {
                    let str = "";
                    for (let i = 0; i < number.length; i++) {
                        if (number[i] == ',') {
                            str += $numberFormat.group;
                        }
                        else if (number[i] == '.') {
                            str += $numberFormat.decimal;
                        }
                        else {
                            str += number[i];
                        }
                    }
                    return str;
                }
                else {
                    return number.replace($app.strToRegex(','), '').replace($app.strToRegex('.'), $numberFormat.decimal)
                }
            }
            return val;
        },
        strToRegex: function (val) {
            var clearVal = val.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
            return new RegExp(clearVal, 'g')
        },
        ajaxComplete: []
    };
}();

$(function () {
    $.validator.addClassRules("date", { });

    $.validator.setDefaults({
        highlight: function (element) {
            $(element).addClass('is-invalid');
            if ($(element).hasClass("select2")) {
                $(element).next(".select2:first").addClass("is-invalid");
            }
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
            if ($(element).hasClass("select2")) {
                $(element).next(".select2:first").removeClass("is-invalid");
            }
        },
        errorElement: 'div',
        errorClass: 'invalid-feedback',
        errorPlacement: function (error, element) {
            if (element.parent('.input-group').length) {
                error.insertAfter(element.parent());
            }
            else {
                if (element.hasClass("select2-hidden-accessible")) {
                    error.insertAfter(element.next(".select2:first"));
                }
                else {
                    error.insertAfter(element);
                }
            }
        }
    });

    $.validator.addMethod("required-group", function (value, element, options) {
        var fields = $(options, element.form);
        var filled_fields = fields.filter(function () {
            return $(this).val() != "";
        });
        //var empty_fields = fields.not(filled_fields);
        // if (filled_fields.length < 1 && empty_fields[0] == element) {
        //     return false;
        // }
        // return true;
        return filled_fields.length;
    }, "Please fill out at least one of these fields.");

    $app.ajaxComplete.push(function () {
        componentTooltip();
        componentPopover();
        componentSelect2();
        componentAutoNumeric();
        componentDate();
        componentInputMask();
    });

    let diff = new Date().getTimezoneOffset();
    if (diff != 0) {
        diff *= -1;
    }
    $app.setCookie("_tzo", diff.toString() , 365);
});

var $lang = {
    code: 'tr',
    deleteAll: 'Tümünü sil',
    selectFile: 'Dosya seç',
    useFile: 'Kullan',
    recordTotals: 'Toplam {0} kayıt',
    select: 'Seçiniz',
    selectEmpty: '--Seçiniz--',
    close: 'Kapat',
    save: 'Kaydet',
    yes: 'Evet',
    no: 'Hayıt',
    cancel: 'İptal',
    upload: 'Yükle',
    errorOccurred: 'Beklenmedik bir hata oluştu',
    excelFile: 'Excel dosyası',
    uploadDone: 'Yükleme başarılı',
    uploadDoneSomeErrors: 'Excel\'deki kayıtlarda hata oluştu.Hata excel\'ini aşağıdakik linkten indirebilirsiniz',
    uploadRunningBackground: 'Excel yükleme işlem arka planda devam ediyor',
    excelErrorFile: 'Hata excel\'i',
    redirecting: 'Yönlenririliyorsunuz...',
    recordNotFound: 'Kayıt bulunamaıd',
    back: 'Geri',
    next: 'İleri',
    deleteConfirm: 'Emin misiniz? Bu işlem daha sonra geri alınamayacaktır!',
    globalConfirm: 'İşleme devam etmek istediğinize emin misiniz?',
    recordSaved: 'İşlem başarılı',
    invalidOperation: 'Geçersiz işlem',
    view: 'Görüntüle',
    pageSize: 'Sayfa boyutu',
    clear: 'Temizle',
    other: 'Diğer',
    mainPage: 'Ana sayfa',
    new: 'Yeni',
    add: 'Ekle',
    noFilter: 'Filtre yok'
};