function saveCustomField(fnDone) {
    var form = $("#frmCustomField");
    var id = form.find(":input[name='Id']").val();
    if (form.valid()) {
        $app.callJx(form.attr("action"), "body", form.serialize(), function (result) {
            if (fnDone && typeof fnDone === "function") {
                fnDone.call(result);
                return;
            }
            $app.toast("success", $lang.recordSaved, function () {
                if (id == "0") {
                    if (history && history.replaceState) {
                        form.find(":input[name='Id']").val(result.data.id);
                        history.replaceState({}, document.title, "/customfields/edit/" + result.data.id);
                    }
                    else {
                        $app.gotoUrl("/customfields/edit/" + id);
                    }

                    $("[data-bs-toggle='tab']").removeClass("disabled");
                    changeGridUrlParams("customFieldId", id);
                    form.find(":input[name='DisplayOrder']").val(result.data.displayOrder);
                }
                form.find(":input[name='DisplayOrder']").val(result.data.displayOrder);
            });
        });
    }
}

$(function () {
    $("#frmCustomField").validate();
});