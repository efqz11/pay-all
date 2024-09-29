
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
var url = $('#url').val();
var isSigned = $('#IsSigned').val() === "True";
// 'https://www.antennahouse.com/hubfs/xsl-fo-sample/pdf/basic-link-1.pdf';


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


    try {
        afterRender();
    } catch (e) {
        console.log('warning! afterRender not found');
    }
}

// render object in pdf form and sidebar
function renderObject(object) {
    var data;
    var message = "";
    switch (object.elementType) {
        case "placeholder":
            data = $.parseHTML(`<div class="resize-drag absolute select-none" data-x="4" data-y="-851" style="transform: translate(4px, -851px);"><div contenteditable="false" spellcheck="false" class="p-2 outline-none whitespace-no-wrap">${object.value}</div></div>`);
            break;

        case "inputField":
            data = $.parseHTML(`<div class="resize-drag absolute select-none" data-x="4" data-y="-851" style="transform: translate(4px, -851px);"><div contenteditable="true" spellcheck="false" class="p-2 outline-none whitespace-no-wrap">${object.value}</div></div>`);
            break;

        case "signature":
            if (object.value !== "" && object.value !== "🡮") {
                data = $.parseHTML(`<div class="resize-drag signature-placeholder absolute select-none" data-x="15" data-y="1" style="transform: translate(15px, 1px);width: 192.8px; height: 88.4937px">
                    <svg width="100%" height="100%" viewBox="0 0 400 150"><path stroke-width="3" stroke-linejoin="round" stroke-linecap="round" stroke="black" fill="none" d="${object.value}"></path></svg>
                        </div>`);
            }
            else {
                data = $.parseHTML(`<div class="resize-drag signature-placeholder absolute select-none" data-x="15" data-y="1" style="transform: translate(15px, 1px);width: 192.8px; height: 88.4937px">
                        <button class="btn btn-primary">Sign</button>
                        </div>`);
            }
            break;

        case "signature-label":
            data = $.parseHTML(`<div class="resize-drag signature absolute select-none" data-x="15" data-y="1" style="transform: translate(15px, 1px);">
                    <svg width="100%" height="100%" viewBox="0 0 400 150"><path stroke-width="3" stroke-linejoin="round" stroke-linecap="round" stroke="black" fill="none" d="${object.value}"></path></svg>
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
        $(data).attr('data-multiline', `${object.multiline}`);
        $(data).attr('style', `transform: translate(${object.canvas_x}px, ${object.canvas_y}px); width: ${object.canvas_width}px; height: ${object.canvas_height}px;`);
    }

    $(data).attr('data-name', `${object.name}`);
    $(data).attr('data-eType', `${object.elementType}`);
    $('#pdf-block').append(data);


    if (object.elementType === "inputField") {
        $('#list-group-fields').append(`<div class="list-group-item list-group-item-action" data-target="${object.name}">
                <span class="drag-handle"><i class="${object.icon}"></i> ${object.name}</span>
                <div class="float-right switch_box box_1">
                    <input type="checkbox" class="switch_1 switch-schedule" value="False" disabled ${object.required ? `checked` : ``}>
                    
                </div>
            </div>`);
    }
    else {
        $('#list-group-placeholders').append(`<div class="list-group-item list-group-item-action" data-target="${object.name}">
                <span class="drag-handle"><i class="${object.icon}"></i> ${object.name}</span>
                <div class="float-right switch_box box_1">
                </div>
            </div>`);
    }

    console.log('object renderred');
}



interact('.resize-drag .signature-placeholder-within')

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
        listeners: { move: dragMoveListener },
        inertia: true,
        modifiers: [
            interact.modifiers.restrictRect({
                restriction: 'parent',
                endOnly: true
            })
        ]
    });


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

function savePdfValues() {
    var values = [];

    $('.resize-drag').each(function (index) {
        var name = $(this).data("name");
        var eType = $(this).data("etype");
        var val = "";
        if (eType.indexOf("signature") >= 0)
            val = $(this).find('svg path').attr('d');
        else // if (eType === "placeholder")
            val = $(this).find('div:first').text();

        values.push({
            "elementType": eType,
            "name": name,
            "value": val
        });
    });


    if (values.map(a => (a.eType === "signature" || a.eType === "placeholder" || a.eType === "inputField") && a.value === "").indexOf(true) >= 0) {
        sendNotification('error', "Kindly fill in all the fields");
        return;
    }

    // prepare post data 
    if (values.length > 0) {
        console.log('values ', values);
        var url = GetAppRootPath() + '/Signature/SaveValues/' + fileId;

        $.ajax({
            contentType: 'application/json',
            type: 'POST',
            url: url,
            data: JSON.stringify(values),
            success: function () {
                sendNotification('success', 'PDF form has been saved updated');
                console.log('data updated.');
            },
            error: handleModalPostFailure
        });
    }
}

function updateObject(objectId, payload) {
    for (var i in allObjects[selectedPageIndex]) {
        if (allObjects[selectedPageIndex][i].name == objectId) {
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
            if (allObjects[i][j].name == objectId) {
                allObjects[i][j].deleted = true;
                break;
            }
        }
    });
}



async function savePdf() {
    await save(allObjects, 'rest.pdf');
}

async function save(objects, name) {
    try {
        var flatten = true;
        const pdfBytes = await saveGetPdfBytes(objects, flatten);
        download(pdfBytes, name, 'application/pdf');
        console.log('PDF downloaded.', pdfBytes);
    } catch (e) {
        console.log('Failed to save PDF.');
        throw e;
    }
}

//async function save(objects, name) {
//    //const PDFLib = await window.getAsset('PDFLib');
//    //const download = await window.getAsset('download');
//    //const makeTextPDF = await window.getAsset('makeTextPDF');
//    let pdfDoc;
//    let form;
//    try {
//        const arrayBuffer = await fetch(url).then(res => res.arrayBuffer());
//        pdfDoc = await PDFLib.PDFDocument.load(arrayBuffer);

//    } catch (e) {
//        console.log('Failed to load PDF.');
//        throw e;
//    }


//    form = pdfDoc.getForm();
//    const fields = form.getFields()
//    fields.forEach(field => {
//        const type = field.constructor.name
//        const name = field.getName()
//        console.log(`${type}: ${name}`)
//    });


//    const pagesProcesses = pdfDoc.getPages().map(async (page, pageIndex) => {

//        const { width, height } = page.getSize();
//        console.log('width', width);
//        console.log('height', height);


//        const pageObjects = objects[pageIndex];
//        // 'y' starts from bottom in PDFLib, use this to calculate y
//        const pageHeight = page.getHeight();
//        const pageWidth = page.getWidth();
//        const embedProcesses = pageObjects.map(async (object) => {
//            if (object.elementType === 'placeholder') {
//                let { name, x, y, value, width, height, placeholder } = object;
//                return () => page.drawText(value, {
//                    x: 50,
//                    y: height - 4 * fontSize,
//                    size: fontSize,
//                    font: timesRomanFont,
//                    color: rgb(0, 0.53, 0.71),
//                });

//                //const superheroField = form.createTextField(name);
//                //superheroField.setText(value);
//                //superheroField.isReadOnly();
//                //return (() => superheroField.addToPage(page, { x, y, width, height }));
//            }
//            else if (object.elementType === 'inputField') {
//                let { name, x, y, value, width, height } = object;
//                const superheroField = form.createTextField(name);
//                superheroField.setText(value);
//                return (() => superheroField.addToPage(page, { x, y, width, height, size: 10 }));
//            }
//            else if (object.elementType === 'image') {
//                let { file, x, y, width, height } = object;
//                let img;
//                try {
//                    if (file.type === 'image/jpeg') {
//                        img = await pdfDoc.embedJpg(await readAsArrayBuffer(file));
//                    } else {
//                        img = await pdfDoc.embedPng(await readAsArrayBuffer(file));
//                    }
//                    return () =>
//                        page.drawImage(img, {
//                            x,
//                            y: pageHeight - y - height,
//                            width,
//                            height,
//                        });
//                } catch (e) {
//                    console.log('Failed to embed image.', e);
//                    return noop;
//                }
//            } else if (object.elementType === 'text') {
//                let { x, y, lines, lineHeight, size, fontFamily } = object;
//                const font = await fetchFont(fontFamily);
//                const [textPage] = await pdfDoc.embedPdf(
//                    await makeTextPDF({
//                        lines,
//                        fontSize: size,
//                        lineHeight,
//                        width: pageWidth,
//                        height: pageHeight,
//                        font: font.buffer || fontFamily, // built-in font family
//                        dy: font.correction(size, lineHeight),
//                    })
//                );
//                return () =>
//                    page.drawPage(textPage, {
//                        width: pageWidth,
//                        height: pageHeight,
//                        x,
//                        y: -y,
//                    });
//            } else if (object.elementType === 'signature') {
//                let { x, y, scale } = object;
//                let path = object.value;
//                const {
//                    pushGraphicsState,
//                    setLineCap,
//                    popGraphicsState,
//                    setLineJoin,
//                    LineCapStyle,
//                    LineJoinStyle,
//                } = PDFLib;
//                return () => {
//                    page.pushOperators(
//                        pushGraphicsState(),
//                        setLineCap(LineCapStyle.Round),
//                        setLineJoin(LineJoinStyle.Round)
//                    );
//                    page.drawSvgPath(path, {
//                        borderWidth: 5,
//                        scale,
//                        x,
//                        y: pageHeight - y,
//                    });
//                    page.pushOperators(popGraphicsState());
//                };
//            }
//            else {
//                return () => console.log('unsupported');
//            }
//        });
//        console.log('embedProcesses', embedProcesses);

//        // embed objects in order
//        const drawProcesses = await Promise.all(embedProcesses);
//        drawProcesses.forEach((p) => p());
//    });
//    await Promise.all(pagesProcesses);
//    try {
//        const pdfBytes = await pdfDoc.save();
//        download(pdfBytes, name, 'application/pdf');
//    } catch (e) {
//        console.log('Failed to save PDF.');
//        throw e;
//    }
//}










// Misc Actions
// --------------

var svg = document.getElementsByTagName('iframe')[0].contentWindow;
function addSignature() {
    MicroModal.show('modal-3');
    svg.clearSignature();
}

function resetSignature() { svg.clearSignature(); }

function addSignatureConfirm() {
    var datapair = svg.getSignature();
    var targetElement = $('.resize-drag[data-etype="signature"][data-name="' + $('#modal-3 #eName').val() + '"]');
    if (targetElement) {
        var w = $(targetElement).width();
        var h = $(targetElement).height();

        $(targetElement).empty();
        $(targetElement).append(`<svg style="" onclick="" width="100%" height="100%" viewBox="0 0 440 150"><path stroke="black" stroke-linejoin="round" stroke-linecap="round" stroke-width="2" fill="none" preserveAspectRatio="xMidYMin slice" d="${datapair}"/></svg>`);
        console.log('targetElement', targetElement, w,h);
        
    }
    MicroModal.close('modal-3');
    console.log('done', datapair);
}


function getIconByType(type) {
    switch (type) {
        case "placeholder":
            return "fad fa-user-friends";
        case "inputField":
            return "fad fa-font";
        case "signature":
            return "fad fa-signature";
        default:
            return "";
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
