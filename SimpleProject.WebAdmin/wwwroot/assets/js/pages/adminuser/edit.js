function adminUserChangePassword(input) {
    if (input.checked) {
        $("#frmAdminUser :input[id^='pwdInput']").removeAttr("disabled");
    }
    else {
        $("#frmAdminUser :input[id^='pwdInput']").val("").attr("disabled", "disabled");
        $("#frmAdminUser :input[name='Password']").val($("#frmAdminUser :input[id^='oldPassword']").val());
    }
}

function adminUserUpdatePassword() {
    $("#frmAdminUser :input[name='Password']").val($("#frmAdminUser :input[id^='pwdInput']").val());
}

function adminUserShowPassword(btn) {
    var input = $(btn).prev(":input");
    if (input.is(":disabled")) {
        return;
    }
    if (input.prop("type") == "text") {
        input.prop("type", "password");
        $(btn).find("i").removeClass("ri-eye-line").addClass("ri-eye-off-line");
    }
    else {
        input.prop("type", "text");
        $(btn).find("i").removeClass("ri-eye-off-line").addClass("ri-eye-line");
    }
}

function saveAdminUser(fnDone) {
    var form = $("#frmAdminUser");
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
                        history.replaceState({}, document.title, "/adminusers/edit/" + result.data.id);
                    }
                    else {
                        $app.gotoUrl("/adminusers/edit/" + id);
                    }
                }
                $(":input[data-field='AdminUserId']").val(result.data.id);
            });
        });
    }
}

$(function () {
    $("#frmAdminUser").validate();
});