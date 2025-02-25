
let pages = [];
var allObjects = [];

var selectedPageIndex = -1;
var scale = 0;
allObjects = pages.map(() => []);


//var pdfData = atob($('#pdfBase64').val());
/*
*  costanti per i placaholder 
*/
var maxPDFx = 595;
var maxPDFy = 842;
var offsetY = 7;

'use strict';


// The workerSrc property shall be specified.
//
pdfjsLib.GlobalWorkerOptions.workerSrc =
    'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.0.943/pdf.worker.min.js';

var fileId = $('#fileId').val();
var url = // $('#url').val();
// 'https://pdf-lib.js.org/assets/dod_character.pdf';
 'https://www.antennahouse.com/hubfs/xsl-fo-sample/pdf/basic-link-1.pdf';


//
// Asynchronous download PDF
//
var loadingTask = pdfjsLib.getDocument({ url }); //{ data: pdfData, renderInteractiveForms : true });
loadingTask.promise.then(function (pdf) {
    
    console.log('page form', pdf);

    const numPages = pdf.numPages;
    pages = Array(numPages)
        .fill()
        .map((_, i) => pdf.getPage(i + 1));
    allObjects = pages.map(() => []);

    //
    // Fetch the first page
    //
    pdf.getPage(1).then(function (page) {
        selectedPageIndex = 0;
        scale = 5;
        var viewport = page.getViewport(scale);
        //
        // Prepare canvas using PDF page dimensions
        //
        var wrapper = document.getElementById('pageContainer');
        var canvas = document.getElementById('the-canvas');
        var context = canvas.getContext('2d');
        canvas.height = viewport.height;
        canvas.width = viewport.width;


        // https://stackoverflow.com/questions/35400722/pdf-image-quality-is-bad-using-pdf-js
        canvas.style.width = "100%";
        canvas.style.height = "100%";
        //wrapper.style.width = Math.floor(viewport.width / scale) + 'pt';
        //wrapper.style.height = Math.floor(viewport.height / scale) + 'pt';
        maxPDFx = Math.floor(viewport.width / scale);
        maxPDFy = Math.floor(viewport.height / scale);
        //
        // Render PDF page into canvas context
        //
        var renderContext = {
            canvasContext: context,
            viewport: viewport
        };
        //page.render(renderContext);


        //page.drawText('Enter your favorite superhero:', { x: 50, y: 700, size: 20 });

        page.render(renderContext).then(function () {
            $(document).trigger("pagerendered");
        }, function () {
            console.log("ERROR");
        });

    });
}, function (reason) {
    alert('error ocurred while loading');
    console.error('error ocurred while loading');
    // PDF loading error
    console.error(reason);
});


/* The dragging code for '.draggable' from the demo above
 * applies to this demo as well so it doesn't have to be repeated. */

// enable draggables to be dropped into this
interact('.dropzone').dropzone({
    // only accept elements matching this CSS selector
    accept: '.drag-drop',
    // Require a 100% element overlap for a drop to be possible
    overlap: 1,

    // listen for drop related events:

    ondropactivate: function (event) {
        // add active dropzone feedback
        event.target.classList.add('drop-active');
    },
    ondragenter: function (event) {
        var draggableElement = event.relatedTarget,
            dropzoneElement = event.target;

        // feedback the possibility of a drop
        dropzoneElement.classList.add('drop-target');
        draggableElement.classList.add('can-drop');
        draggableElement.classList.remove('dropped-out');
        //draggableElement.textContent = 'Dragged in';
    },
    ondragleave: function (event) {
        // remove the drop feedback style
        event.target.classList.remove('drop-target');
        event.relatedTarget.classList.remove('can-drop');
        event.relatedTarget.classList.add('dropped-out');
        //event.relatedTarget.textContent = 'Dragged out';
    },
    ondrop: function (event) {
        //event.relatedTarget.textContent = 'Dropped';
    },
    ondropdeactivate: function (event) {
        // remove active dropzone feedback
        event.target.classList.remove('drop-active');
        event.target.classList.remove('drop-target');
    }
});

interact('.drag-drop')
    .draggable({
        inertia: true,
        restrict: {
            restriction: "#selectorContainer",
            endOnly: true,
            elementRect: { top: 0, left: 0, bottom: 1, right: 1 }
        },
        autoScroll: true,
        // dragMoveListener from the dragging demo above
        onmove: dragMoveListener,
    });

interact('.resize-drag')
    .resizable({
        // resize from all edges and corners
        //edges: { left: true, right: true, bottom: true, top: true },
        edges: { right: '[resize-edges~=right]', bottom: '[resize-edges~=bottom]' },

        listeners: {
            move(event) {
                var target = event.target
                var x = (parseFloat(target.getAttribute('data-x')) || 0)
                var y = (parseFloat(target.getAttribute('data-y')) || 0)

                // update the element's style
                target.style.width = event.rect.width + 'px'
                target.style.height = event.rect.height + 'px'

                // translate when resizing from top or left edges
                x += event.deltaRect.left
                y += event.deltaRect.top

                target.style.webkitTransform = target.style.transform =
                    'translate(' + x + 'px,' + y + 'px)'

                target.setAttribute('data-x', x)
                target.setAttribute('data-y', y)
                //target.textContent = Math.round(event.rect.width) + '\u00D7' + Math.round(event.rect.height)
            }
        },
        modifiers: [
            // keep the edges inside the parent
            interact.modifiers.restrictEdges({
                outer: 'parent',
                endOnly: true,
            }),

            // minimum size
            interact.modifiers.restrictSize({
                min: { width: 100, height: 30 }
            })
        ],

        inertia: true
    })
    .draggable({
        listeners: { move: window.dragMoveListener },
        inertia: true,
        modifiers: [
            interact.modifiers.restrictRect({
                restriction: 'parent',
                endOnly: true
            })
        ]
    });


interact('.resize-drag-svg')
    .resizable({
        // resize from all edges and corners
        edges: { left: true, right: true, bottom: true, top: true },
        //edges: { right: '[resize-edges~=right]', bottom: '[resize-edges~=bottom]' },

        listeners: {
            move(event) {
                var target = event.target
                var x = (parseFloat(target.getAttribute('data-x')) || 0)
                var y = (parseFloat(target.getAttribute('data-y')) || 0)

                // update the element's style
                target.style.width = event.rect.width + 'px'
                target.style.height = event.rect.height + 'px'

                // translate when resizing from top or left edges
                x += event.deltaRect.left
                y += event.deltaRect.top

                target.style.webkitTransform = target.style.transform =
                    'translate(' + x + 'px,' + y + 'px)'

                target.setAttribute('data-x', x)
                target.setAttribute('data-y', y)
                //target.textContent = Math.round(event.rect.width) + '\u00D7' + Math.round(event.rect.height)
            }
        },
        modifiers: [
            // keep the edges inside the parent
            interact.modifiers.restrictEdges({
                outer: 'parent',
                endOnly: true,
            }),

            // minimum size
            interact.modifiers.restrictSize({
                min: { width: 100, height: 30 }
            })
        ],

        inertia: true
    })
    .draggable({
        listeners: { move: window.dragMoveListener },
        inertia: true,
        modifiers: [
            interact.modifiers.restrictRect({
                restriction: 'parent',
                endOnly: true
            })
        ]
    })


//interact('.resizable')
//    .draggable({
//        inertia: true,
//        onmove: dragMoveListener,
//        //function(event) {
//        //    var rectangle = event.target;

//        //    rectangle.x += event.dx;
//        //    rectangle.y += event.dy;
//        //    //rectangle.draw();
//        //}
//    })
//    .resizable({
//        edges: { left: true, right: true, top: true, bottom: true },
//        invert: 'reposition',
//        //restrict: {
//        //restriction: 'svg',
//        //elementRect: { top: 1, left: 1, bottom: 1, right: 1 }
//        //},
//        onmove: function (event) {
//            var rectangle = event.target;

//            rectangle.x = event.rect.left;
//            rectangle.y = event.rect.top;
//            rectangle.w = event.rect.width;
//            rectangle.h = event.rect.height;
//            //rectangle.draw();
//        }
//    });



function dragMoveListener(event) {
    var target = event.target,
        // keep the dragged position in the data-x/data-y attributes
        x = (parseFloat(target.getAttribute('data-x')) || 0) + event.dx,
        y = (parseFloat(target.getAttribute('data-y')) || 0) + event.dy;

    // translate the element
    target.style.webkitTransform =
        target.style.transform = 'translate(' + x + 'px, ' + y + 'px)';

    // update the posiion attributes
    target.setAttribute('data-x', x);
    target.setAttribute('data-y', y);
}

// this is used later in the resizing demo
window.dragMoveListener = dragMoveListener;

$(document).bind('pagerendered', function (e) {
    $('#pdfManager').show();
    console.log('rendered ', this);
    var parametri = JSON.parse($('#parameters').val());
    //$('#parametriContainer').empty();
    renderFieldsFromDb(0);
});

// render place holders on page
function renderFieldsFromDb(currentPageIndx) {
    var params = JSON.parse($('#filedFromDb').val());
    if (params === null || params === undefined)
        return;

    var placeholderCount = 0;
    var inputCount = 0;
    var signatureCount = 0;
    for (i = 0; i < params.length; i++) {
        if ((params[i].page-1) === currentPageIndx) {
            var object = {
                elementType: params[i].elementType,

                name: params[i].name,
                fieldType: params[i].type,
                placeholder: params[i].placeholder,
                required: params[i].required,
                multiline: params[i].multiline,
                x: params[i].x,
                y: params[i].y,
                height: params[i].height,
                width: params[i].width,
                value: params[i].value,
                icon: params[i].iconn,
                deleted: params[i].deleted,
            };

            // push from objects to allObjects[] initally
            allObjects[currentPageIndx].push(object);

            // render object from db json
            renderObject(params[i]);

            if (object.elementType === "inputField")
                inputCount++;
            else if (object.elementType === "signature")
                signatureCount++;
            else
                placeholderCount++;
        }
    }

    if (allObjects[currentPageIndx].length > 0) {
        var text = "";
        if (inputCount > 0)
            text = `${inputCount} input fields`;
        if (signatureCount > 0)
            text += ` · ${signatureCount} signatures`;
        if (placeholderCount > 0)
            text += ` · ${placeholderCount} placeholders`;
        $('#nimifa_div').append(text);
    }

}

// render object in pdf form and sidebar
function renderObject(object) {
    var data;
    var message = "";
    switch (object.elementType) {
        case "placeholder":
            data = $.parseHTML(`<div class="resize-drag absolute select-none" data-x="4" data-y="-851" style="transform: translate(4px, -851px);"><div contenteditable="false" spellcheck="false" class="p-2 outline-none whitespace-no-wrap">${object.placeholder}</div><div resize-edges="bottom right">🡮</div></div>`);
            break;

        case "inputField":
            data = $.parseHTML('<div class="resize-drag absolute select-none" data-x="4" data-y="-851" style="transform: translate(4px, -851px);"><div contenteditable="true" spellcheck="false" class="p-2 outline-none whitespace-no-wrap"></div><div resize-edges="bottom right">🡮</div></div>');
            break;


        case "signature":
            data = $.parseHTML(`<div class="resize-drag signature-placeholder absolute select-none" data-x="4" data-y="-851" style="transform: translate(4px, -851px);width: 192.8px; height: 88.4937px">
                        <button class="btn btn-primary">Sign</button>
                        <div resize-edges="bottom right">🡮</div>
                        </div>`);
            break;

        case "signature-label":
            data = $.parseHTML(`<div class="resize-drag signature absolute select-none" data-x="4" data-y="-851" style="transform: translate(4px, -851px);">
            <svg width="100%" height="100%" viewBox="0 0 400 150"><path stroke-width="3" stroke-linejoin="round" stroke-linecap="round" stroke="black" fill="none" d="${object.value}"></path></svg>
            <div resize-edges="bottom right">🡮</div>
        </div>`);
            break;

        default:
            sendNotification('error', "Unabled to add fields");
            return;
    }

    // update x,y, height and width
    if (object.canvas_x && object.canvas_y) {
        $(data).attr('data-x', object.canvas_x);
        $(data).attr('data-y', object.canvas_y);
        $(data).attr('style', `transform: translate(${object.canvas_x}px, ${object.canvas_y}px); width: ${object.canvas_width}px; height: ${object.canvas_height}px;`);
    }

    $(data).attr('data-name', `${object.name}`);
    $(data).attr('data-eType', `${object.elementType}`);
    $(data).attr('data-multiline', `${object.multiline}`);
    $('#pdf-block').append(data);


    if (object.elementType === "inputField" || object.elementType === "signature") {
        $('#list-group-fields').append(`<div class="list-group-item list-group-item-action" data-target="${object.name}">
                <span class="drag-handle"><i class="${object.icon}"></i> ${object.name}</span>
                <div class="float-right switch_box box_1">
                    <input type="checkbox" class="switch_1 switch-schedule" value="False" ${object.required ? `checked` : ``}>
                    <span id="remove" class="remove"><i class="fad fa-trash"></i></span>
                </div>
            </div>`);
    }
    else {
        $('#list-group-placeholders').append(`<div class="list-group-item list-group-item-action" data-target="${object.name}">
                <span class="drag-handle"><i class="${object.icon}"></i> ${object.name}</span>
                <div class="float-right switch_box box_1">
                    <span id="remove" class="remove"><i class="fad fa-trash"></i></span>
                </div>
            </div>`);
    }

    console.log('object renderred ', object.name);
}



// change [age]
function changePage(disabled, currentPage, delta) {
    if (disabled) {
        return;
    }

    /*recupera solo i parametri non posizionati in pagina*/
    var parametri = [];
    $(".drag-drop.dropped-out").each(function () {
        var valore = $(this).data("valore");
        var descrizione = $(this).find(".descrizione").text();
        parametri.push({ valore: valore, descrizione: descrizione, posizioneX: -1000, posizioneY: -1000 });
        $(this).remove();
    });

    //svuota il contentitore
    $('#pager').remove();
    currentPage += delta;
    renderizzaPlaceholder(currentPage, parametri);
    //aggiorna lo stato dei pulsanti
    //aggiorna gli elementi visualizzati
}

function showCoordinates() {
    var validi = [];
    var nonValidi = [];
    var pagedConfig = [];
    $('.btn-save-pdf').html(getLoaderSmWhiteHtml());
    var maxHTMLx = $('#the-canvas').width();
    var maxHTMLy = $('#the-canvas').height();
    var paramContainerWidth = $('#parametriContainer').width();
    // retrieves all valid placholders  (recupera tutti i placholder validi)
    $('.resize-drag').each(function (index) {
        var x = parseFloat($(this).data("x"));
        var y = parseFloat($(this).data("y"));
        var name = $(this).data("name");
        var multiline = $(this).data("multiline");
        var elementType = $(this).data("etype");
        var required = $('.lgg .list-group-item[data-target="' + name + '"]').find('.switch-schedule').prop('checked');
        var deleted = $('.lgg .list-group-item[data-target="' + name + '"]').data('deleted') || false;

        var _val = $(this).find('div:first').text();
        var _hc = parseFloat($(this).height());
        var _h = _hc * maxPDFy / maxHTMLy;
        var _wc = parseFloat($(this).width());
        var _w = _wc * maxPDFx / maxHTMLx;

        // y = (y * maxPdfy / maxHtmly)
        var posizioneX = x * maxPDFx / maxHTMLx;
        var posizioneY = (y * -1) * maxPDFy / maxHTMLy;
        //posizioneX -= _w;


        // ignore height size reduce for signature fields
        if (elementType.indexOf("signature") < 0)
            posizioneY -= _h;

        // for all placehodlers and input fields (not readonly) => reduce height by 10px
        if (elementType === "placeholder" || elementType === "inputField")
            if (!multiline)
                _h -= 9;

        //var pdfY = y * maxPDFy / maxHTMLy;
        //var posizioneY = maxPDFy - offsetY - pdfY;
        //var posizioneX = (x * maxPDFx / maxHTMLx) - paramContainerWidth;

        //posizioneX = x;
        //posizioneY = y * -1;
        var val = { "name": name, "X": posizioneX, "Y": posizioneY, "elementType": elementType, "h": _h, "w": _w };
        validi.push(val);

        updateObject(name, { x: posizioneX, y: posizioneY, value: _val, height: _h, width: _w, required });
        var _ = {
            "page": (selectedPageIndex + 1),
            "elementType": elementType,
            "name": name,
            "height": _h,
            "width": _w,
            "value": _val,
            "x": posizioneX,
            "y": posizioneY,
            "placeholder": _val,
            "required": required,
            "multiline": multiline,
            "deleted": deleted,
            "icon": getIconByType(elementType),

            "canvas_width": _wc,
            "canvas_height": _hc,
            "canvas_x": x,
            "canvas_y": y,
        };
        if (elementType === 'signature-label') {
            _.value = $(this).find('svg path').attr('d');
        }
        pagedConfig.push(_);
    });

    // prepare post data 
    if (pagedConfig.length > 0) {
        console.log('post data', pagedConfig);
        var url = GetAppRootPath() + '/Signature/Configure/' + fileId;

        $.ajax({
            contentType: 'application/json',
            type: 'POST',
            url: url,
            data: JSON.stringify(pagedConfig),
            success: function () {
                sendNotification('success', 'PDF form elements have been updated');
                console.log('data updated.');
            },
            error: handleModalPostFailure,
            complete: function () {
                $('.btn-save-pdf').html("Save PDF Fields");
            }
        });
    }

    if (validi.length === 0) {
        alert('No placeholder dragged into document');
    }
}

function updateObject(objectId, payload) {
    for (var i in allObjects[selectedPageIndex]) {
        if (allObjects[selectedPageIndex][i].name === objectId) {
            allObjects[selectedPageIndex][i].height = payload.height;
            allObjects[selectedPageIndex][i].width = payload.width;
            allObjects[selectedPageIndex][i].value = payload.value;
            allObjects[selectedPageIndex][i].x = payload.x;
            allObjects[selectedPageIndex][i].y = payload.y;
            break; //Stop this loop, we found it!
        }
    }
}

function removeObject(objectId) {
    $(pages).each(function (i, e) {
        for (var j in allObjects[i]) {
            if (allObjects[i][j].name === objectId) {
                allObjects[i][j].deleted = true;
                break;
            }
        }
    });
}





// Misc Actions
// --------------

function addSignature() {
    MicroModal.show('modal-3');
    svg.clearSignature();
}

function resetSignature() { svg.clearSignature(); }

function addSignatureConfirm() {
    var datapair = svg.getSignature();
    if (datapair === undefined || datapair === "") {
        sendNotification('error', 'Please enter your signature');
        return;
    }

    //alert('done');

    var object = {
        elementType: 'signature-label',
        name: Date.now(),
        x: 0.0,
        y: 0.0,
        height: 0.0,
        width: 0.0,
        value: datapair,
        deleted: false,
        required: false,
        multiline:false
        //path: datapair
    };

    object.icon = getIconByType(object.elementType);
    renderObject(object);

    sendNotification('success', 'Signature label was added');
    MicroModal.close('modal-3');


    allObjects = allObjects.map((objects, pIndex) =>
        pIndex === selectedPageIndex ? [...objects, object] : objects
    );
    console.log('done', datapair);
}

function addFormElement(type) {
    $('#modal-2').find('.rats').hide();
    $('#modal-2').find('.' + type).show();
    $('#modal-2 #ElementType').val(type);

    // clear form data
    $('#modal-2 #FieldType').val(0);
    $('#modal-2 #Name').val("");

    if (type === "signature-label") {
        addSignature();
        return;
    }
    
    switch (type) {
        case 'signature':
        case 'signature-label':
            $('#modal-2 #FieldType').val(type);
            break;
        case 'placeholder':
            $('#modal-2 #FieldType').val(type);
            break;
        case 'inputField':
            $('#modal-2 #FieldType').val("TextField");
            break;
        default:
    }

    MicroModal.show('modal-2');
}

function addFormElemeantConfirm() {
    var inputValues = $('#modal-2').find(':input,select,:checkbox').serializeArray();
    console.log('form submit', inputValues);


    var object = {
        elementType: $('#modal-2 #ElementType').val(),

        name: $('#modal-2 #Name').val(),
        fieldType: $('#modal-2 #FieldType').val(),
        placeholder: "{" + $('#modal-2 #Placeholder').val() + "}",
        required: $('#modal-2 #IsRequired').prop('checked'),
        multiline: $('#modal-2 #IsMultiline').prop('checked') || false,
        x: 0.0,
        y: 0.0,
        height: 0.0,
        width: 0.0,
        value: "",
        deleted: false,
    };
    object.icon = getIconByType(object.elementType);
    if (object.name === "" || object.name === undefined) {
        sendNotification('error', "Name field cannnot be empty"); return;
    }

    if (allObjects.map(a => a.name === object.name).indexOf(true) >= 0) {
        sendNotification('error', "Name field must be unique for all elements"); return;
    }


    var isError = false;
    var data;
    var message = "";
    switch (object.elementType) {
        case "placeholder":
            if (object.placeholder === "0" || object.placeholder === "")
                isError = true;

            if (!isError) {
                message = object.placeholder + ' placeholder was added';
            }
            break;

        case "inputField":
            if (object.fieldType === "0" || object.fieldType === "")
                isError = true;

            if (!isError) {
                message = 'New input field was added';
            }
            break;


        case "signature":

            if (!isError) {
                message = 'New Signature field was added';
            }
            break;

        default:
            sendNotification('error', "Unabled to add fields");
            return;
    }

    if (isError)
        sendNotification('error', "Please fill in all the fields");
    else {
        console.log(object);
        renderObject(object);
        
        sendNotification('success', message);
        MicroModal.close('modal-2');



        allObjects = allObjects.map((objects, pIndex) =>
            pIndex === selectedPageIndex ? [...objects, object] : objects
        );
    }
}



function addTextField(text = "New Text Field") {
    const id = genID();
    //fetchFont(currentFont);
    const object = {
        id,
        text,
        type: "text",
        size: 16,
        lineHeight: 1.4,
        //fontFamily: currentFont,
        x: 0,
        y: 0
    };
    allObjects = allObjects.map((objects, pIndex) =>
        pIndex === selectedPageIndex ? [...objects, object] : objects
    );
}

async function savePdf() {
    await save(allObjects, 'rest213.pdf');
}


async function save(objects, name) {
    try {
        const pdfBytes = await saveGetPdfBytes(objects);
        download(pdfBytes, name, 'application/pdf');
        console.log('PDF downloaded.', pdfBytes);
    } catch (e) {
        console.log('Failed to save PDF.');
        throw e;
    }
}

function InitializeSortable() {
    $("#list-group-fields").sortable({
        items: 'div.list-group-item',
        cursor: 'move',
        axis: 'y',
        dropOnEmpty: false,
        placeholder: "ui-state-highlight",
        handle: ".drag-handle",
        start: function (e, ui) {
            //prevAdjustmentIdOrder = $('#tblMasterPayAdjustments input:hidden.pay-adj-Id').map(function (e) {
            //    return $(this).val();
            //}).get();
            //console.log(prevAdjustmentIdOrder);
            //ui.item.addClass("selected");
        },
        stop: function (e, ui) {
            //neORder = [];
            //ui.item.removeClass("selected");
            //var postData = $(this).find("tr").map(function (index) {
            //    return {
            //        CalculationOrder: index,
            //        Id: $(this).find('input:hidden.pay-adj-Id').val()
            //    }
            //}).get();
            //console.log('postData');
            //console.log(postData);
            //convertToLoadingTable('#tblMasterPayAdjustments')

            //$(this).find("tr").map(function (index) {
            //    if ($(this).find('input.calc-order').length > 0) {
            //        $(this).find('input.calc-order').val(index);
            //    }
            //    //if (index > 0) {
            //    //    $(this).find("td").eq(2).html(index);
            //    //}
            //});

            //var url = GetAppRootPath() + "/PayAdjustment/UpdateOrder";
            //$.post(url, { modelsJson: JSON.stringify(postData) }, function (data) { })
            //    .done(function () { location.reload(); });
        }
    });

    $("#list-group-fields").disableSelection();
}
