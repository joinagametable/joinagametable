function closeMessageFlash(elementId) {
    $("#" + elementId).remove();
}

// ------------------------------------------------------------------------- //
var GameTableMetaDataEditor = {

    // Handle all SimpleMDE instances
    __currentSimpleMdeInstances: new Map(),

    // Initialize game table meta data editor
    initialize: function () {

        // Initialize SimpleMDE for all textarea
        $("textarea").each(function () {
            GameTableMetaDataEditor.__createSimpleMdeInstance(this);
        });

        // Hook metadata type change to set the right input for the value
        $('#metadata select').on('change', function () {
            GameTableMetaDataEditor.__changeFieldType(this);
        });

        // Disable form submission with "enter"
        $(document).on("keydown", "form", function (event) {
            return event.key !== "Enter";
        });

        // Success
        console.info("GameTableMetaDataEditor Initialized");
    },

    // Add new meta data entry
    addNewEntry: function () {
        var idx = GameTableMetaDataEditor.__incrementIndex();
        var metadataTemplate = $("#metadata-template").html().replace(/{{idx}}/g, idx);
        $("#metadata-add-item").before(metadataTemplate);
        GameTableMetaDataEditor.__addSelectListener(idx);

        return false;
    },

    // Remove meta data entry
    removeEntry: function (idx) {
        $("#metadata-item-" + idx).remove();
        GameTableMetaDataEditor.__rewriteIndex();
        return false;
    },

    __createSimpleMdeInstance: function(element) {
        var simpleMDEInstance = new SimpleMDE({
            element: element,
            spellChecker: false,
            promptURLs: true,
            status: false,
            insertTexts: {
                table: ["", "\n\n| Column 1 | Column 2 | Column 3 |\n| -------- | -------- | -------- |\n| Text     | Text      | Text     |\n\n"]
            },
            shortcuts: {
                drawTable: "Cmd-Alt-T"
            },
            showIcons: ["code", "table"],
            hideIcons: ["side-by-side", "fullscreen", "image"]
        });

        GameTableMetaDataEditor.__currentSimpleMdeInstances.set(element.id, simpleMDEInstance);
    },

    __changeFieldType: function (instance) {
        var fieldId = 'MetaData_' + parseInt(instance.name.substr(instance.name.indexOf('[') + 1)) + '__Value';
        switch (instance.value) {
            case "SYNOPSIS":
            case "GAME_MASTER_WORD":
            case "GAME_FACILITATOR_WORD":
                // Replace element with textarea
                var currentDocument = document.getElementById(fieldId);
                var textarea = $(document.createElement('textarea'));
                textarea.attr("id", fieldId);
                textarea.attr("name", currentDocument.name);
                textarea.text(currentDocument.value);
                $(currentDocument).replaceWith(textarea);

                // Initialize Markdown editor
                GameTableMetaDataEditor.__createSimpleMdeInstance(document.getElementById(fieldId));
                break;
            default:
                try {
                    // Delete SimpleMDE instance
                    GameTableMetaDataEditor.__currentSimpleMdeInstances.get(fieldId).toTextArea();
                    GameTableMetaDataEditor.__currentSimpleMdeInstances.delete(fieldId);
                } catch (error) {
                } finally {
                    // Replace element with textarea
                    var currentDocument = document.getElementById(fieldId);
                    var inputText = $(document.createElement('input'));
                    inputText.attr("type", "text");
                    inputText.attr("id", fieldId);
                    inputText.attr("name", currentDocument.name);
                    inputText.val(currentDocument.value);
                    $(currentDocument).replaceWith(inputText);
                }
                break;
        }
    },

    __incrementIndex: function () {
        var metadata = $("#metadata");
        var idx = metadata.data("index");
        metadata.data("index", idx + 1);

        return idx;
    },

    __setIndex: function (value) {
        var metadata = $("#metadata");
        metadata.data("index", value);
    },

    __addSelectListener: function (idx) {
        $('#MetaData_' + idx + '__Key').on('change', function () {
            GameTableMetaDataEditor.__changeFieldType(this);
        });
    },

    __rewriteIndex: function () {
        var keyPattern = [
            'MetaData_{{idx}}__Key',
            'MetaData[{{idx}}].Key'
        ];
        var valuePattern = [
            'MetaData_{{idx}}__Value',
            'MetaData[{{idx}}].Value'
        ];
        var idx = 0;
        $('.metadata-entry').each(function () {
            var currentInputKey = $("#" + this.id + " select");
            currentInputKey.attr("id", keyPattern[0].replace(/{{idx}}/g, idx));
            currentInputKey.attr("name", keyPattern[1].replace(/{{idx}}/g, idx));

            var currentInputValue = $("#" + this.id + " input");
            currentInputValue.attr("id", valuePattern[0].replace(/{{idx}}/g, idx));
            currentInputValue.attr("name", valuePattern[1].replace(/{{idx}}/g, idx));

            idx += 1;
        });
        GameTableMetaDataEditor.__setIndex(idx);
    }
};
