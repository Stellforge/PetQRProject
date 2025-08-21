function saveCustomFieldOption(fnDone) {
    var form = $("#frmCustomFieldOption");
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
                        history.replaceState({}, document.title, "/customfieldoptions/edit/" + result.data.id);
                    }
                    else {
                        $app.gotoUrl("/customfieldoptions/edit/" + id);
                    }
                }
                form.find(":input[name='DisplayOrder']").val(result.data.displayOrder);
            });
        });
    }
}

$(function () {
    $("#frmCustomFieldOption").validate();
});