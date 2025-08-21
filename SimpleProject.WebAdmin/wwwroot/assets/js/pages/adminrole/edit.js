function generateUniqueId() {
    return 'id-' + Date.now() + '-' + Math.floor(Math.random() * 1000);
}

function renderMenuItems(data, isCustom = false) {
    const container = $(`#${isCustom ? 'customMenuItems' : 'defaultMenuItems'}`);
    let lastParentItem = null;

    data.forEach(item => {
        const listItem = $('<div>', {
            class: 'widget mb-2 p-2',
            'data-id': generateUniqueId(),
            'data-name': item.name || '',
            'data-icon': item.icon || '',
            'data-title': item.title || '',
            'data-lang-key': item.langKey || '',
            'data-url': item.url || '',
            'data-target': item.target || '',
            'data-level': item.level || 0,
            'data-is-header': item.isHeader || "false"
        });

        item.level = item.level || 0;
        const existsInLeftMenu = leftMenu.some(leftMenuItem => leftMenuItem.name === item.name);

        if (existsInLeftMenu) {
            listItem.addClass('default-menu-item');
        }
        else {
            listItem.addClass('custom-menu-item');
        }

        const content = $('<div>').append(
            $('<i>', { class: 'ri-drag-move-line me-2' }),
            document.createTextNode(item.title)
        );
        listItem.append(content);

        setItemLevel(listItem[0], item.level);

        if (!item.isHeader) {
            if (isCustom) {
                listItem.addClass('d-flex justify-content-between');
                const controls = $('<div>', { class: 'invisible d-flex' }).append(
                    $('<span>', { class: 'clickable me-2 move-right' }).append(
                        $('<i>', { class: 'ri-corner-down-right-line"' })
                    ),
                    $('<span>', { class: 'clickable me-2 move-left' }).append(
                        $('<i>', { class: 'ri-corner-up-left-line' })
                    )
                );

                if (!existsInLeftMenu) {
                    controls.append(
                        $('<span>', { class: 'clickable me-2 text-danger remove-item' }).append(
                            $('<i>', { class: 'ri-delete-bin-line' })
                        ),
                        $('<span>', { class: 'clickable me-2 text-warning edit-item' }).append(
                            $('<i>', { class: 'ri-edit-box-line' })
                        )
                    );
                }

                listItem.append(controls);
            }
        }

        if (item.isHeader) {
            if (isCustom && !existsInLeftMenu) {
                listItem.addClass('d-flex justify-content-between header-item');

                const controls = $('<div>', { class: 'invisible d-flex' }).append(
                    $('<span>', { class: 'clickable me-2 text-danger remove-item' }).append(
                        $('<i>', { class: 'ri-delete-bin-line' })
                    ),
                    $('<span>', { class: 'clickable me-2 text-warning edit-item' }).append(
                        $('<i>', { class: 'ri-edit-box-line' })
                    )
                );

                listItem.append(controls);
            }
        }

        container.append(listItem);
        addButtonEventListeners(listItem[0]);
    });
}

function makeSortable(el) {
    new Sortable(el, {
        group: {
            name: 'nested',
            pull: true,
            put: function (to, from, draggedEl) {
                const isDraggingHeader = draggedEl.classList.contains('header-item') || getItemIsHeader(draggedEl);
                const isTargetHeaderContainer = to.el.classList.contains('header-item') || getItemIsHeader(to.el);

                if (isDraggingHeader) {
                    if (to.el.id === 'customMenuItems' || to.el.id === 'defaultMenuItems') {
                        return true;
                    }
                    return false;
                }

                if ((!isDraggingHeader && isTargetHeaderContainer) || getItemIsHeader(to.el.parentElement)) {
                    return false;
                }
                const nestedCustomItems = draggedEl.querySelectorAll('.custom-menu-item');
                for (let item of nestedCustomItems) {
                    if (to.el.id === 'defaultMenuItems' || to.el.closest('#defaultMenuItems')) {
                        return false;
                    }
                }
                return true;
            }
        },
        animation: 150,
        fallbackOnBody: true,
        swapThreshold: 0.65,
        onEnd: function (event) {
            updateItemStyles(event.item);
            updateItemLevel(event.item);
        }
    });
}

function updateItemStyles(item) {
    let $item = $(item);
    let isItemHeader = getItemIsHeader(item);
    const existsInLeftMenu = leftMenu.some(leftMenuItem => leftMenuItem.name === $item.data('name'));
    const isCustom = $item.parent().attr('id') === "customMenuItems";

    if (isCustom) {
        $item.addClass('d-flex justify-content-between');
        if (!isItemHeader && $item.find('.move-right, .remove-item').length === 0) {
            const $controls = $('<div>', { class: 'invisible d-flex' }).append(
                $('<span>', { class: 'clickable me-2 move-right' }).append(
                    $('<i>', { class: 'ri-corner-down-right-line' })
                ),
                $('<span>', { class: 'clickable me-2 move-left' }).append(
                    $('<i>', { class: 'ri-corner-up-left-line' })
                )
            );
            if (!existsInLeftMenu) {
                $controls.append(
                    $('<span>', { class: 'clickable me-2 text-danger remove-item' }).append(
                        $('<i>', { class: 'ri-delete-bin-line' })
                    ),
                    $('<span>', { class: 'clickable me-2 text-warning edit-item' }).append(
                        $('<i>', { class: 'ri-edit-box-line' })
                    )
                );
            }
            $item.append($controls);
        }
        else if (isItemHeader && !existsInLeftMenu && $item.find('.remove-item').length === 0) {
            const $controls = $('<div>', { class: 'invisible d-flex' }).append(
                $('<span>', { class: 'clickable me-2 text-danger remove-item' }).append(
                    $('<i>', { class: 'ri-delete-bin-line' })
                ),
                $('<span>', { class: 'clickable me-2 text-warning edit-item' }).append(
                    $('<i>', { class: 'ri-edit-box-line' })
                )
            );
            $item.append($controls);
        }

        addButtonEventListeners(item);
    }
    else {
        $item.removeClass('d-flex justify-content-between header-item');
        $item.find('.invisible.d-flex').remove();
    }
}

function updateItemLevel(item) {
    var parentElement = item.previousElementSibling || item.parentElement;
    var newLevel = 0;
    var parentElementLevel = getLevel(parentElement);
    var itemLevel = getLevel(item);

    const itemIsHeader = getItemIsHeader(item);

    if (parentElement.id === "customMenuItems" || parentElement.id === "defaultMenuItems" || itemIsHeader) {
        newLevel = 0;
    }

    if (itemIsHeader) {
        let nextElement = item.nextSibling;
        let childElementLevel = 0
        while (nextElement) {
            setItemLevel(nextElement, childElementLevel);
            nextElement = nextElement.nextSibling || null;
            childElementLevel = getLevel(nextElement) || 0;
        }
    }

    if (itemLevel !== newLevel && !itemIsHeader) {
        setItemLevel(item, newLevel);
    }
}

function setItemLevel(item, level) {
    const isHeader = item.dataset.isHeader === "true";
    if (level >= 0 && level <= 1 && !isHeader) {
        item.dataset.level = level;
        item.style.marginLeft = `${marginLeftValueConstant * level}${marginLeftUnitConstant}`;
    }
}

function addButtonEventListeners(listItem) {
    const leftBtn = $(".move-left").off("click").on("click", (event) => {
        event.preventDefault();
        moveMenuItemLeft(listItem);
    });

    const rightBtn = $(".move-right").off("click").on("click", (event) => {
        event.preventDefault();
        moveMenuItemRight(listItem);
    });
    const removeBtn = $(".remove-item").off("click").on("click", (event) => {
        event.preventDefault();
        removeMenuItem(listItem);
    });

    const editBtn = $(".edit-item").off("click").on("click", (event) => {
        event.preventDefault();
        editMenuItem(listItem);
    });
}

function moveMenuItemLeft(item) {
    const previousItem = item.previousElementSibling;
    const itemIsHeader = getItemIsHeader(item);
    const previousItemIsHeader = getItemIsHeader(previousItem);
    if (itemIsHeader && previousItemIsHeader) {
        return;
    }

    const currentLevel = getLevel(item);
    if (currentLevel > 0 && currentLevel <= 1) {
        setItemLevel(item, currentLevel - 1);
    }

}

function moveMenuItemRight(item) {
    const previousItem = item.previousElementSibling;
    const isPreviousItemHeader = getItemIsHeader(previousItem);
    const isItemHeader = getItemIsHeader(item);

    if (isItemHeader || isPreviousItemHeader) {
        return;
    }

    const currentLevel = getLevel(item);
    if (previousItem && !isPreviousItemHeader) {
        if (currentLevel >= 0 && currentLevel < 1)
            setItemLevel(item, currentLevel + 1);
    }
}

function editMenuItem(item) {
    const itemDetails = {
        id: item.dataset.id,
        name: item.dataset.name,
        title: item.dataset.title,
        langKey: item.dataset.langKey,
        url: item.dataset.url,
        target: item.dataset.target,
        icon: item.dataset.icon,
        isHeader: getItemIsHeader(item)
    };
    showEditMenuItemModal(itemDetails);
}

function removeMenuItem(item) {
    if (item.classList.contains('custom-menu-item')) {
        item.parentElement.removeChild(item);
    }
    saveAdminRole();
}

function getItemIsHeader(item) {
    return item.dataset.isHeader === "true" ? true : false;
}

function getLevel(item) {
    let level = 0;
    if (item) {
        level = parseInt(item.getAttribute('data-level'));
    }
    return level;
}

function getMenuSettings() {
    var useDefaultMenu = $("#Settings_UseDefaultMenu").is(":checked");
    var result;

    if (useDefaultMenu) {
        result = {
            UseDefaultMenu: true,
            Menus: []
        };
    }
    else {
        result = {
            UseDefaultMenu: false,
            Menus: getCustomMenuData()
        };
    }

    return result;
}

function getCustomMenuData() {
    var menuData = [];

    $("#customMenuItems .widget").each(function () {
        var item = {
            Name: $(this).data("name"),
            Title: $(this).data("title"),
            LangKey: $(this).data("lang-key"),
            Target: $(this).data("target"),
            Url: $(this).data("url"),
            Icon: $(this).data("icon"),
            Level: $(this).data("level"),
            IsHeader: $(this).hasClass("header-item")
        };

        var filteredItem = Object.fromEntries(
            Object.entries(item).filter(([_, value]) => value != null || value != 0)
        );

        menuData.push(filteredItem);
    });

    return menuData;
}

function isItemExists(property, propertyValue) {
    const existingItem = [...document.querySelectorAll('#customMenuItems .widget, #defaultMenuItems .widget')]
        .some(item => item.dataset[property].toLowerCase() === propertyValue.toLowerCase());
    return existingItem;
}

function createMenuItemForm() {
    const form = $("<form>", { id: 'createMenuItem' });
    form.append(
        $("<div>", { class: "p-3" }).append(
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "newMenuItemName", html: "Name", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "text", id: "newMenuItemName", class: "form-control", id: "newMenuItemName" })
                )
            ),
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "newMenuItemTitle", html: "Title", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "text", id: "newMenuItemTitle", class: "form-control" })
                )
            ),
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "newMenuItemLangKey", html: "Language Key", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "text", id: "newMenuItemLangKey", class: "form-control" })
                )
            ),
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "newMenuItemUrl", html: "URL", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "text", id: "newMenuItemUrl", class: "form-control" })
                )
            ),
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "newMenuItemTarget", html: "Target", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "text", id: "newMenuItemTarget", class: "form-control" })
                )
            ),
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "newMenuItemIcon", html: "Icon", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "hidden", id: "newMenuItemIcon", class: "form-control" }),
                    $("<div>", { id: "icon-list-wrapper", style: "max-height: 200px; overflow-y: auto; border: 1px solid #ced4da; padding: 5px;" }).append(
                        $("<div>", { id: "iconlar", style: "display: flex; flex-wrap: wrap; gap: 10px;" })
                    ),
                )
            ),

            $("<div>", { class: "row mb-3 form-check" }).append(
                $("<div>", { class: "col-lg-9 offset-lg-3" }).append(
                    $("<input>", { type: "checkbox", id: "newMenuItemIsHeader", class: "form-check-input" }),
                    $("<label>", { for: "newMenuItemIsHeader", class: "form-check-label", html: "Is Header" })
                )
            )
        )
    );
    return form;
}

function getIcons(selectedIcon) {
    console.log(selectedIcon)
    $.ajax({
        url: '/assets/icons.json',
        type: 'GET',
        dataType: 'json',
        success: function (icons) {
            icons.forEach(function (icon) {
                const iconElement = $("<i>", {
                    class: `${icon.font} lh-1 fs-2 icon-select`,
                    "data-icon-name": icon.font
                });

                if (icon.font === selectedIcon) {
                    iconElement.addClass('selected-icon');
                }

                iconElement.click(function () {
                    $('#newMenuItemIcon').val(icon.font);
                    $('#iconlar i').removeClass('selected-icon');
                    $(this).addClass('selected-icon');
                });

                $('#iconlar').append(iconElement);
            });
        },
        error: function (error) {
            console.error("Error fetching JSON data:", error);
        }
    });
}

function handleSaveButtonClick() {
    const id = generateUniqueId();
    const name = $('#newMenuItemName').val();
    const title = $('#newMenuItemTitle').val();
    const langKey = $('#newMenuItemLangKey').val();
    const url = $('#newMenuItemUrl').val();
    const target = $('#newMenuItemTarget').val();
    const icon = $('#newMenuItemIcon').val();
    const isHeader = $('#newMenuItemIsHeader').prop('checked');

    if (isItemExists("name", name)) {
        return;
    }

    createNewMenuItem(id, name, title, langKey, url, target, icon, isHeader);
    $('.modal').modal('hide');
}

function createNewMenuItem(id, name, title, langKey, url, target, icon, isHeader) {
    const container = $('#customMenuItems');
    const listItem = $('<div>', {
        class: 'widget mb-2 p-2 custom-menu-item d-flex justify-content-between',
        'data-id': id,
        'data-name': name,
        'data-title': title,
        'data-lang-key': langKey,
        'data-url': url,
        'data-target': target,
        'data-icon': icon,
        'data-is-header': isHeader,
        'data-level': 0
    });

    const content = $('<div>').append(
        $('<i>', { class: 'ri-drag-move-line me-2' }),
        document.createTextNode(title)
    );
    listItem.append(content);

    const controls = $('<div>', { class: 'invisible d-flex' });

    if (isHeader) {
        listItem.addClass('header-item');
        controls.append(
            $('<span>', { class: 'clickable me-2 text-danger remove-item' }).append(
                $('<i>', { class: 'ri-delete-bin-line' })
            ),
            $('<span>', { class: 'clickable me-2 text-warning edit-item' }).append(
                $('<i>', { class: 'ri-edit-box-line' })
            )
        );
    }
    else {
        controls.append(
            $('<span>', { class: 'clickable me-2 move-right' }).append(
                $('<i>', { class: 'ri-corner-down-right-line' })
            ),
            $('<span>', { class: 'clickable me-2 move-left' }).append(
                $('<i>', { class: 'ri-corner-up-left-line' })
            ),
            $('<span>', { class: 'clickable me-2 text-danger remove-item' }).append(
                $('<i>', { class: 'ri-delete-bin-line' })
            ),
            $('<span>', { class: 'clickable me-2 text-warning edit-item' }).append(
                $('<i>', { class: 'ri-edit-box-line' })
            )
        );
    }

    listItem.append(controls);
    container.append(listItem);

    addButtonEventListeners(listItem[0]);
}

function handleCancelButtonClick() {
    $('.modal').modal('hide');
}

function showCreateNewItemModel(event) {
    const createMenuItemModalParams = {
        type: 'primary',
        body: createMenuItemForm(),
        fn: handleSaveButtonClick,
        fnClose: handleCancelButtonClick
    };

    var modal = $app.showModal(createMenuItemModalParams.type, null, createMenuItemModalParams.body, createMenuItemModalParams.fn, createMenuItemModalParams.fnClose);
    modal.find(".modal-dialog").addClass("modal-lg");
    getIcons();
}

function createEditMenuItemForm(item) {
    const form = $("<form>", { id: 'editMenuItemForm' });
    form.append(
        $("<div>", { class: "p-3" }).append(
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "editMenuItemName", html: "Name", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "text", id: "editMenuItemName", class: "form-control", value: item.name }),
                    $("<input>", { type: "hidden", id: "editMenuItemId", class: "form-control", value: item.id })
                )
            ),
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "editMenuItemTitle", html: "Title", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "text", id: "editMenuItemTitle", class: "form-control", value: item.title })
                )
            ),
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "editMenuItemLangKey", html: "Language Key", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "text", id: "editMenuItemLangKey", class: "form-control", value: item.langKey })
                )
            ),
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "editMenuItemUrl", html: "URL", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "text", id: "editMenuItemUrl", class: "form-control", value: item.url })
                )
            ),
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "editMenuItemTarget", html: "Target", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "text", id: "editMenuItemTarget", class: "form-control", value: item.target })
                )
            ),
            $("<div>", { class: "row mb-3" }).append(
                $("<label>", { for: "editMenuItemIcon", html: "Icon", class: "col-form-label col-lg-3" }),
                $("<div>", { class: "col-lg-9" }).append(
                    $("<input>", { type: "hidden", id: "editMenuItemIcon", class: "form-control", value: item.icon }),
                    $("<div>", { id: "icon-list-wrapper", style: "max-height: 200px; overflow-y: auto; padding: 5px;" }).append(
                        $("<div>", { id: "iconlar", style: "display: flex; flex-wrap: wrap; gap: 10px;" })
                    ),
                )
            ),
            $("<div>", { class: "row mb-3 form-check" }).append(
                $("<div>", { class: "col-lg-9 offset-lg-3" }).append(
                    $("<input>", { type: "checkbox", id: "editMenuItemIsHeader", class: "form-check-input", checked: Boolean(item.isHeader === "true" || item.isHeader === true) }),
                    $("<label>", { for: "editMenuItemIsHeader", class: "form-check-label", html: "Is Header" })
                )
            )
        )
    );
    return form;
}

function showEditMenuItemModal(item) {
    const editMenuItemModalParams = {
        type: 'primary',
        body: createEditMenuItemForm,
        fn: handleUpdateButtonClick,
        fnClose: handleCancelButtonClick
    };
    const populatedForm = createEditMenuItemForm(item);
    var modal = $app.showModal(editMenuItemModalParams.type, null, populatedForm, editMenuItemModalParams.fn, editMenuItemModalParams.fnClose);
    modal.find(".modal-dialog").addClass("modal-lg");
    getIcons(item.icon);
}

function handleUpdateButtonClick() {
    const id = $('#editMenuItemId').val();
    const name = $('#editMenuItemName').val();
    const title = $('#editMenuItemTitle').val();
    const langKey = $('#editMenuItemLangKey').val();
    const url = $('#editMenuItemUrl').val();
    const target = $('#editMenuItemTarget').val();
    const icon = $('#editMenuItemIcon').val();
    const isHeader = $('#editMenuItemIsHeader').prop('checked');
    const item = {
        id, name, title, langKey, url, target, icon, isHeader
    };

    updateMenuItem(item);
    $('.modal').modal('hide');
}

function updateMenuItem(updatedItem) {
    const listItem = $(`[data-id="${updatedItem.id}"]`);

    if (listItem.length === 1) {
        const element = listItem[0];

        element.dataset.id = updatedItem.id;
        element.dataset.name = updatedItem.name || '';
        element.dataset.title = updatedItem.title || '';
        element.dataset.langKey = updatedItem.langKey || '';
        element.dataset.url = updatedItem.url || '';
        element.dataset.target = updatedItem.target || '';
        element.dataset.icon = updatedItem.icon || '';
        element.dataset.isHeader = updatedItem.isHeader;

        $(element).empty();

        const content = $('<div>').append(
            $('<i>', { class: 'ri-drag-move-line me-2' }),
            updatedItem.name
        );
        const controls = $('<div>', { class: 'invisible d-flex' });

        console.log(updatedItem.isHeader);
        if (updatedItem.isHeader) {
            controls.append(
                $('<span>', { class: 'clickable me-2 text-danger remove-item' }).append(
                    $('<i>', { class: 'ri-delete-bin-line' })
                ),
                $('<span>', { class: 'clickable me-2 text-warning edit-item' }).append(
                    $('<i>', { class: 'ri-edit-box-line' })
                )
            );
        }
        else {
            controls.append(
                $('<span>', { class: 'clickable me-2 move-right' }).append(
                    $('<i>', { class: 'ri-corner-down-right-line' })
                ),
                $('<span>', { class: 'clickable me-2 move-left' }).append(
                    $('<i>', { class: 'ri-corner-up-left-line' })
                ),
                $('<span>', { class: 'clickable me-2 text-danger remove-item' }).append(
                    $('<i>', { class: 'ri-delete-bin-line' })
                ),
                $('<span>', { class: 'clickable me-2 text-warning edit-item' }).append(
                    $('<i>', { class: 'ri-edit-box-line' })
                )
            )
        }

        $(element).append(content).append(controls);

        if (updatedItem.isHeader) {
            element.classList.add('header-item');
        }
        else {
            element.classList.remove('header-item');
        }

        addButtonEventListeners(element);
    }
    else {
        $app.toast("danger", $lang.recordSaved, function () {
            console.log("Güncellenecek bir eleman bulunamadı");
        });
    }
}

function checkForDefaultSettings() {
    var checkbox = $("#Settings_UseDefaultMenu");
    var customSettings = $("#customSettings");

    if (checkbox.is(":checked")) {
        customSettings.addClass("d-none");
    }
    else {
        customSettings.removeClass("d-none");
    }
}

function saveAdminRole(fnDone) {
    var form = $("#frmAdminRole");
    var id = form.find(":input[name='Id']").val();

    var customMenuData = getMenuSettings();
    form.find(":input[name='SettingsRaw']").val(JSON.stringify(customMenuData));

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
                        history.replaceState({}, document.title, "/adminroles/edit/" + result.data.id);
                    }
                    else {
                        $app.gotoUrl("/adminroles/edit/" + id);
                    }
                }
            });
        });
    }
}

$(function () {
    $("#frmAdminRole").validate();

    renderMenuItems(filteredLeftMenu);
    if (customMenu.menus) {
        renderMenuItems(customMenu.menus, true);
    }

    makeSortable(document.getElementById('defaultMenuItems'));
    makeSortable(document.getElementById('customMenuItems'));
    checkForDefaultSettings();

    document.addEventListener('dragover', function (event) {
        let hoveredElement = event.target.closest('.list-group-item');
        if (hoveredElement) {
            let nestedList = createNestedList(hoveredElement);
            if (!Sortable.get(nestedList)) {
                makeSortable(nestedList);
            }
        }
    });
});