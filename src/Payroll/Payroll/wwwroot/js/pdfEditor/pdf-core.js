

$(function () {

    $(document).on('mouseover', '.lgg .list-group-item', function (e) {
        $('#pdf-block').find('div.resize-drag[data-name="' + $(this).data('target') + '"]').addClass('found');
    });
    $(document).on('mouseleave', '.lgg .list-group-item', function (e) {
        $('#pdf-block').find('div.resize-drag[data-name="' + $(this).data('target') + '"]').removeClass('found');
    });

    $(document).on('click', '.resize-drag[data-etype="signature"] .btn', function (e) {
        var ename = $(this).parents('.resize-drag').data('name');
        $('#modal-3 #eName').val(ename);
        addSignature();
    });
});

function getIconByType(type) {
    switch (type) {
        case "placeholder":
            return "fad fa-user-friends";
        case "inputField":
            return "fad fa-font";
        case "signature":
            return "fad fa-file-signature";
        case "signature-label":
            return "fad fa-signature";
        default:
            return "";
    }
}

async function saveGetPdfBytes(objects, flatten) {
    flatten = flatten || false;
    console.log('flatten .', flatten);
    let { rgb } = PDFLib.rgb;
    //const PDFLib = await window.getAsset('PDFLib');
    //const download = await window.getAsset('download');
    //const makeTextPDF = await window.getAsset('makeTextPDF');
    let pdfDoc;
    let form;
    try {
        const arrayBuffer = await fetch(url).then(res => res.arrayBuffer());
        pdfDoc = await PDFLib.PDFDocument.load(arrayBuffer);

    } catch (e) {
        console.log('Failed to load PDF.');
        throw e;
    }


    form = pdfDoc.getForm();
    const fields = form.getFields()
    fields.forEach(field => {
        const type = field.constructor.name
        const name = field.getName()
        console.log(`${type}: ${name}`);

        var widgets = field.acroField.getWidgets();
        widgets.forEach((w) => {
            const rect = w.getRectangle();
            console.log(rect,  w);
        });
    });

    PDFLib.PDFFont.MAX_FONT_SIZE = 10;
    const helvetica = await pdfDoc.embedFont(PDFLib.StandardFonts.Helvetica);

    const pagesProcesses = pdfDoc.getPages().map(async (page, pageIndex) => {
        
        const { width, height } = page.getSize();
        console.log('width', width);
        console.log('height', height);


        const pageObjects = objects[pageIndex];
        // 'y' starts from bottom in PDFLib, use this to calculate y
        const pageHeight = page.getHeight();
        const pageWidth = page.getWidth();
        const embedProcesses = pageObjects.map(async (object) => {
            if (object.elementType === 'placeholder') {
                let { name, x, y, value, width, height, placeholder } = object;
                const superheroField = form.createTextField(name);
                superheroField.setText(placeholder);
                superheroField.enableReadOnly();

                //return () =>
                //    page.drawTextField({
                //        width, height, x, y, fontSize: 10, padding: 2
                //    });
                superheroField.addToPage(page, { x, y, width, height, font: helvetica });
                if (flatten)
                    form.getTextField(name).setText(object.value);
                return () => console.log('added placeholder field');
            }
            else if (object.elementType === 'inputField') {
                let { name, x, y, value, width, height, readonly, multiline } = object;
                const superheroField = form.createTextField(name);
                superheroField.setText(value);
                superheroField.setMaxLength(100);
                if (multiline)
                    superheroField.enableMultiline();
                superheroField.addToPage(page, { x, y, width, height, size: 10 });
                if (flatten)
                    form.getTextField(name).setText(object.value);
                return () => console.log('added input field');
            }
            else if (object.elementType === 'image') {
                let { file, x, y, width, height } = object;
                let img;
                try {
                    if (file.type === 'image/jpeg') {
                        img = await pdfDoc.embedJpg(await readAsArrayBuffer(file));
                    } else {
                        img = await pdfDoc.embedPng(await readAsArrayBuffer(file));
                    }
                    return () =>
                        page.drawImage(img, {
                            x,
                            y: pageHeight - y - height,
                            width,
                            height
                        });
                } catch (e) {
                    console.log('Failed to embed image.', e);
                    return noop;
                }
            } else if (object.elementType === 'text') {
                let { x, y, lines, lineHeight, size, fontFamily } = object;
                const font = await fetchFont(fontFamily);
                const [textPage] = await pdfDoc.embedPdf(
                    await makeTextPDF({
                        lines,
                        fontSize: size,
                        lineHeight,
                        width: pageWidth,
                        height: pageHeight,
                        font: font.buffer || fontFamily, // built-in font family
                        dy: font.correction(size, lineHeight),
                    })
                );
                return () =>
                    page.drawPage(textPage, {
                        width: pageWidth,
                        height: pageHeight,
                        x,
                        y: -y,
                    });
            } else if (object.elementType === 'signature-label') {
                let { x, y } = object;
                let path = object.value;
                let scale = 0.5;
                const svgPath = 'M 0,20 L 100,160 Q 130,200 150,120 C 190,-40 200,200 300,150 L 400,90'
                const {
                    pushGraphicsState,
                    setLineCap,
                    popGraphicsState,
                    setLineJoin,
                    LineCapStyle,
                    LineJoinStyle,
                } = PDFLib;
                return () => {
                    //page.pushOperators(
                    //    pushGraphicsState(),
                    //    setLineCap(LineCapStyle.Round),
                    //    setLineJoin(LineJoinStyle.Round)
                    //);
                    // Set fill color and opacity
                    page.drawSvgPath(path, {
                        x,
                        y,
                        //color: PDFLib.rgb(0, 0, 1),
                        scale,
                        borderWidth: 3,
                        borderLineCap: 1
                    });

                    console.log('svg path draw, ', object);
                    //page.pushOperators(popGraphicsState());
                };
            }
            else {
                return () => console.log('unsupported element type: ', object.elementType);
            }
        });
        console.log('embedProcesses', embedProcesses);

        // embed objects in order
        const drawProcesses = await Promise.all(embedProcesses);
        drawProcesses.forEach((p) => p());
    });
    await Promise.all(pagesProcesses);


    if (flatten) {
        console.log('flattenning PDF.');
        try {
            const arrayBuffer = await pdfDoc.save();
            pdfDoc = await PDFLib.PDFDocument.load(arrayBuffer);

            
            form = pdfDoc.getForm();
            const fields = form.getFields();
            fields.forEach(field => {
                field.setText('XXX');
            });


            pdfDoc.getPages().map((page, pageIndex) => {

                const pageObjects = objects[pageIndex];
                $(pageObjects).each(function (i, e) {
                    if (e.elementType === "inputField" || e.elementType === "placeholder") {
                        const field = form.getTextField(e.name);
                        field.setText(e.value);
                    }
                });
            });

            return await pdfDoc.save();
        } catch (e) {
            console.log('Failed to load PDF.');
            throw e;
        }
    }
    return await pdfDoc.save();
}

