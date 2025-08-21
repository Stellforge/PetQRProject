var fileBrowserClass = function (el) {
    if ($(el).data("file-browser")) {
        return $(el).data("file-browser");
    }

    var element = $(el), $this = this, container, modal;

    function setup() {
        element.wrap($("<div>", { class: "input-group" }));
        container = element.parent();

        container.append(
            $("<div>", { class: "input-group-text" }).append(
                $("<i>", { class: "ri-image-line lh-1 fs-6" })
            ),
            $("<button>", { type: "button", class: "btn btn-primary" }).append(
                $("<i>", { class: "ri-folder-open-line lh-1 fs-6" })
            )
        );

        new bootstrap.Popover(container.find(".input-group-text").get(0), {
            container: "body",
            trigger: "hover",
            html: true,
            placement: "bottom",
            content: function () {
                if ($app.isEmpty(element.val())) {
                    return $lang.selectFile;
                }
                return $("<img>", { class: "img-thumbnail", src: element.val() })
            }
        });

        container.find("button").on("click", function () {
            modal = $app.showModal(undefined, undefined, "<div id='fileList'></div>", function () {
                var item = $("#fileList :input[type='checkbox']:checked");
                if (item.length) {
                    element.val(item.parents("tr:first").data("url"));
                    modal.modal("hide");
                }
            }, function () {
                modal = undefined;
            }, function () {
                $("#fileList").append(
                    $("<div>", { class: "card custom-card" }).append(
                        $("<div>", { class: "card-body p-0" }).append(
                            $("<div>", { class: "d-flex p-3 align-items-center justify-content-between border-bottom" }).append(
                                $("<div>", { class: "flex-fill" }).append(
                                    $("<h6>", { class: "fw-medium mb-0 path-label", text: "~/"})
                                ),
                                $("<div>", { class: "d-flex justify-content-end", id: "headerButtonContainer" }).append(
                                    $("<button>", { type: "button", class: "btn btn-primary mx-1 create-folder" }).append(
                                        $("<i>", { class: "ri-add-circle-line me-1" }),
                                        "Create folder"
                                    ),
                                    $("<button>", { type: "button", class: "btn btn-primary mx-1 upload-file" }).append(
                                        $("<i>", { class: "ri-upload-line me-1" }),
                                        "Upload file"
                                    )
                                )
                            ),
                            $("<form>", { id: "createFolderContainer", class: "p-3 d-none" }).append(
                                $("<div>", { class: "row gy-3" }).append(
                                    $("<div>", { class: "col-12" }).append(
                                        $("<label>", { class: "form-label", text: "Folder name" }),
                                        $("<input>", { type: "text", name: "name", class: "form-control" }),
                                        $("<input>", { type: "hidden", name: "path" })
                                    ),
                                    $("<div>", { class: "col-12 text-end" }).append(
                                        $("<button>", { type: "button", class: "btn btn-dark mx-1 cancel-button" }).append(
                                            $("<i>", { class: "ri-close-line me-1" }),
                                            "Cancel"
                                        ),
                                        $("<button>", { type: "button", class: "btn btn-primary mx-1 create-folder-ok" }).append(
                                            $("<i>", { class: "ri-save-line me-1" }),
                                            "Create"
                                        )
                                    )
                                )
                            ),
                            $("<form>", { id: "uploadFieContainer", class: "p-3 d-none" }).append(
                                $("<div>", { class: "row gy-3" }).append(
                                    $("<div>", { class: "col-12" }).append(
                                        $("<input>", { type: "text", class: "file-upload", name: "image", "data-file-upload": "true" }),
                                        $("<input>", { type: "hidden", name: "path" })
                                    ),
                                    $("<div>", { class: "col-12 text-end" }).append(
                                        $("<button>", { type: "button", class: "btn btn-dark mx-1 cancel-button" }).append(
                                            $("<i>", { class: "ri-close-line me-1" }),
                                            "Cancel"
                                        ),
                                        $("<button>", { type: "button", class: "btn btn-primary mx-1 upload-file-ok" }).append(
                                            $("<i>", { class: "ri-save-line me-1" }),
                                            "Upload"
                                        )
                                    )
                                )
                            ),
                            $("<form>", { id: "renameContainer", class: "p-3 d-none" }).append(
                                $("<div>", { class: "row gy-3" }).append(
                                    $("<div>", { class: "col-12" }).append(
                                        $("<label>", { class: "form-label", text: "File name" }),
                                        $("<input>", { type: "text", name: "name", class: "form-control" }),
                                        $("<input>", { type: "hidden", name: "path" })
                                    ),
                                    $("<div>", { class: "col-12 text-end" }).append(
                                        $("<button>", { type: "button", class: "btn btn-dark mx-1 cancel-button" }).append(
                                            $("<i>", { class: "ri-close-line me-1" }),
                                            "Cancel"
                                        ),
                                        $("<button>", { type: "button", class: "btn btn-primary mx-1 rename-file-ok" }).append(
                                            $("<i>", { class: "ri-save-line me-1" }),
                                            "Rename"
                                        )
                                    )
                                )
                            ),
                            $("<div>", { class: "table-responsive", id: "tableContainer" }).append(
                                $("<table>", { class: "table text-nowrap table-hover" }).append(
                                    $("<thead>").append(
                                        $("<tr>").append(
                                            $("<th>", { text: "", style: "width:2rem;" }),
                                            $("<th>", { text: "File Name" }),
                                            $("<th>", { text: "Size" }),
                                            //$("<th>", { text: "Date" }),
                                            $("<th>", { text: "Actions", style: "width:10rem;" })
                                        )
                                    ),
                                    $("<tbody>", { class: "files" })
                                )
                            )
                        )
                    )
                );

                modal.find(".create-folder").on("click", function () {
                    $("#headerButtonContainer").addClass("d-none");
                    $("#tableContainer").addClass("d-none");
                    $("#createFolderContainer").removeClass("d-none");
                    modal.find(".modal-footer").addClass("d-none");
                    $("#createFolderContainer :input[name='path']").val($("#fileList").attr("data-path"));
                });

                modal.find(".upload-file").on("click", function () {
                    $("#headerButtonContainer").addClass("d-none");
                    $("#tableContainer").addClass("d-none");
                    $("#uploadFieContainer").removeClass("d-none");
                    modal.find(".modal-footer").addClass("d-none");
                    $("#uploadFieContainer :input[name='path']").val($("#fileList").attr("data-path"));
                });

                modal.find(".cancel-button").on("click", function () {
                    $("#headerButtonContainer").removeClass("d-none");
                    $("#tableContainer").removeClass("d-none");
                    $("#createFolderContainer").addClass("d-none");
                    $("#uploadFieContainer").addClass("d-none");
                    $("#renameContainer").addClass("d-none");
                    modal.find(".modal-footer").removeClass("d-none");
                });

                modal.find(".create-folder-ok").on("click", function () {
                    $app.callJx("/home/createfolder", "body", $("#createFolderContainer").serialize(), function (result) {
                        modal.find(".cancel-button:first").trigger("click");
                        loadFiles();
                    });
                });

                modal.find(".rename-file-ok").on("click", function () {
                    $app.callJx("/home/renamefile", "body", $("#renameContainer").serialize(), function (result) {
                        modal.find(".cancel-button:first").trigger("click");
                        loadFiles();
                    });
                });

                modal.find(".upload-file-ok").on("click", function () {
                    $app.postJx("/home/uploadfile", "body", $("#uploadFieContainer"), function (result) {
                        modal.find(".cancel-button:first").trigger("click");
                        loadFiles();
                    });
                });

                $("#tableContainer").on("click", ".rename-file", function () {
                    var tr = $(this).parents("tr:first");
                    if (tr.length) {
                        $("#headerButtonContainer").addClass("d-none");
                        $("#tableContainer").addClass("d-none");
                        $("#renameContainer").removeClass("d-none");
                        modal.find(".modal-footer").addClass("d-none");
                        $("#renameContainer :input[name='path']").val(tr.data("url"));

                        var names = tr.data("url").split("/");
                        $("#renameContainer :input[name='name']").val(names[names.length - 1]);
                    }
                });

                $("#tableContainer").on("click", ".delete-file", function () {
                    var tr = $(this).parents("tr:first");
                    if (tr.length) {
                        $app.confirm($lang.deleteConfirm, function () {
                            $app.callJx("/home/deletefile", "body", { path: tr.data("url") }, function (result) {
                                loadFiles();
                            });
                        });
                    }
                });

                $("#tableContainer").on("click", "a[data-url]", function () {
                    var url = $(this).attr("data-url");
                    loadFiles(url);
                });

                loadFiles();
            });
            modal.find(".modal-dialog").removeClass("modal-lg").addClass("modal-xl");
            modal.find(".ok-btn").html("<i class='ri-check-double-line me-2'></i> " + $lang.useFile);
        });
    }

    function loadFiles(path) {
        var fileList = $("#fileList");

        if (path) {
            fileList.attr("data-path", path);
        }

        fileList.find(".files").empty();
        $app.callJx("/home/getfiles", fileList, { path: fileList.attr("data-path") }, function (result) {

            result.data = $.grep(result.data, function (value) {
                return !$app.isEmpty(value[2]);
            });

            $.each(result.data, function (i, e) {
                var tr = $("<tr>");

                if (e[0] == "dir") {
                    tr.append($("<td>"));
                    tr.append(
                        $("<td>").append(
                            $("<a>", { href: "javascript:;", "data-url": e[2], class: "link-primary" }).append(
                                $("<i>", { class: "ri-folder-open-line lh-1 fs-6 me-1" }),
                                $("<span>", { text: e[1] })
                            )
                        )
                    );
                }
                else {
                    var icon = "ri-file-line";
                    var image = false;

                    var ext = e[1].indexOf(".") >= 0 ? e[1].split('.').pop().toLowerCase() : "";
                    if (ext == "zip" || ext == "rar" || ext == "7z" || ext == "tar") {
                        icon = "ri-file-zip-line";
                    }
                    else if (ext == "csv") {
                        icon = "ri-file-excel-line";
                    }
                    else if (ext == "pdf") {
                        icon = "ri-file-pdf-line";
                    }
                    else if (ext == "xls" || ext == "xlsx" || ext == "xlsm" || ext == "xlsb" || ext == "xltx") {
                        icon = "ri-file-excel-line";
                    }
                    else if (ext == "doc" || ext == "docx" || ext == "docm" || ext == "dot" || ext == "rtf") {
                        icon = "ri-file-word-line";
                    }
                    else if (ext == "ppt" || ext == "pptx" || ext == "pptm" || ext == "ppt") {
                        icon = "ri-file-ppt-line";
                    }
                    else if (ext == "jpeg" || ext == "jpg" || ext == "gif" || ext == "tiff" || ext == "psd" || ext == "png") {
                        icon = "ri-file-image-line";
                        image = true;
                    }
                    else if (ext == "cs" || ext == "html" || ext == "html" || ext == "c" || ext == "cpp" || ext == "php" || ext == "js" || ext == "json") {
                        icon = "ri-file-code-line";
                    }
                    else if (ext == "mp4" || ext == "mov" || ext == "wmv" || ext == "flv" || ext == "avi" || ext == "avchd" || ext == "webm" || ext == "mkv") {
                        icon = "ri-file-video-line";
                    }
                    else if (ext == "m4a" || ext == "flac" || ext == "mp3" || ext == "wav" || ext == "wma" || ext == "aac") {
                        icon = "ri-file-music-line";
                    }

                    tr.append(
                        $("<td>").append(
                            $("<div>", { class: "form-check" }).append(
                                $("<input>", { type: "checkbox", class: "form-check-input" })
                            )
                        )
                    );

                    tr.append(
                        $("<td>").append(
                            $("<span>", { class: "file-name" }).append(
                                $("<i>", { class: icon + " lh-1 fs-6 me-1" }),
                                $("<span>", { text: e[1] })
                            )
                        )
                    );

                    if (image) {
                        new bootstrap.Popover(tr.find(".file-name").get(0), {
                            container: "body",
                            trigger: "hover",
                            html: true,
                            placement: "bottom",
                            content: $("<img>", { class: "img-thumbnail", src: e[2] })
                        });
                    }

                    tr.find(".form-check-input").on("change", function () {
                        if (this.checked) {
                            $("#fileList .form-check-input").prop("checked", false);
                            this.checked = true;
                        }
                    });

                    tr.on("click", function () {
                        $(this).find(".form-check-input").prop("checked", true).trigger("change");
                    });
                }

                tr.append($("<td>", { html: (e[3] > 0 ? getSize(e[3]) : "") }));
                //tr.append($("<td>", { html: e[4] }));
                if (e[2] != "/media") {
                    tr.append(
                        $("<td>").append(
                            $("<div>", { class: "btn-list text-nowrap" }).append(
                                $("<a>", { href: e[2], download: e[1], class: "btn btn-sm btn-icon btn-primary1-light", title:"download" }).append(
                                    $("<i>", { class: "ri-download-line" })
                                ),
                                $("<button>", { class: "btn btn-sm btn-icon btn-primary-light rename-file", title: "rename" }).append(
                                    $("<i>", { class: "ri-input-field" })
                                ),
                                $("<button>", { class: "btn btn-sm btn-icon btn-danger-light delete-file", title:"delete" }).append(
                                    $("<i>", { class: "ri-delete-bin-line" })
                                )
                            )
                        )
                    );
                }
                else {
                    tr.append("");
                }

                tr.data("url", e[2]);
                tr.data("file-type", e[0]);

                fileList.find(".files").append(tr);
            });

            fileList.find(".path-label").text(fileList.attr("data-path")?.replace("/media", "~"));
        });
    }

    function getSize(bytes) {
        if (bytes == 0) { return "0.00 B"; }
        var e = Math.floor(Math.log(bytes) / Math.log(1024));
        return (bytes / Math.pow(1024, e)).toFixed(2) + ' ' + ' KMGTP'.charAt(e) + 'B';
    }


    this.imagePath = function (path) {
        element.val(path);
    }

    element.data("file-browser", this);
    setup();
}

function $fileBrowser(el) {
    var file = $(el).data("file-browser");
    if (file) {
        return file;
    }
    return new fileBrowserClass(el);
}

$(function () {
    $("[data-file-select]").each(function () {
        $fileBrowser(this);
    });
    $app.ajaxComplete.push(function () {
        $("[data-file-select]").each(function () {
            $fileBrowser(this);
        });
    });
});