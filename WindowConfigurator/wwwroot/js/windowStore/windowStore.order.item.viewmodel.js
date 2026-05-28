var windowStore = windowStore || {};
windowStore.order = windowStore.order || {};
windowStore.order.item = windowStore.order.item || {};

windowStore.order.item.ItemViewModel = function () {
    var self = this;
    self.orderId = "";
    self.frameSelected = ko.observable(true);
    self.priceCalc = new windowStore.order.item.PriceCalculator();

    //
    // If you put the ko.mapping stuff at the bottom of this class
    // then you might be able to avoid the ifs here
    //

    self.sectionWidthAppliesToCurrent = true;
    self.sectionWidthAppliesToOutside = false;
    self.sectionWidthAppliesToAll = false;
    self.sectionWidthInternalCallCount = 0;

    self.styleAppliesToCurrent = true;
    self.styleAppliesToOutside = false;
    self.styleAppliesToAll = false;

    self.grilleAppliesToCurrent = true;
    self.grilleStyleAppliesToOutside = false;
    self.grilleStyleAppliesToAll = false;

    self.sdlAppliesToCurrent = true;
    self.sdlAppliesToOutside = false;
    self.sdlAppliesToAll = false;

    self.validator = new windowStore.order.item.ItemValidator(self);
    self.measurementValidator = new windowStore.measurement.measurementValidator();

    self.onProductLineSelected = function (productLine) {
        ko.mapping.fromJS(productLine, {}, self.productLine);

        // We also need to make sure the sections have the proper style for the new product line

        for (var i = 0; i < self.sections().length; i++) {
            var section = self.sections()[i];

            // Find the sections current style in the new product line

            var style = null;
            for (var j = 0; j < self.productLine.operationalStyles().length; j++) {
                var productStyle = self.productLine.operationalStyles()[j];
                if (productStyle.name() === section.style.name()) {
                    style = productStyle;
                    break;
                }

                // Picture style applies to all productlines

                if (productStyle.name() === "Picture") {
                    style = productStyle;
                }
            }

            ko.mapping.fromJS(style, {}, section.style);
        }

        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    }

    self.onFrameColorSelected = function (frameColor) {
        ko.mapping.fromJS(frameColor, {}, self.frameColor);
        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };
    self.onJambDepthSelected = function (jambDepth) {
        ko.mapping.fromJS(jambDepth, {}, self.jambDepth);
        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };
    self.onPaneConfugurationSelected = function (paneConfiguration) {
        ko.mapping.fromJS(paneConfiguration, {}, self.paneConfiguration);
        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };
    self.onBrickmouldStyleSelected = function (brickmouldStyle) {
        ko.mapping.fromJS(brickmouldStyle, {}, self.brickmouldStyle);
        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.onWidthChanged();
        self.onHeightChanged();
        self.updatePreview();
    };
    self.onBrickmouldColorSelected = function (brickmouldColor) {
        ko.mapping.fromJS(brickmouldColor, {}, self.brickmouldColor);
        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };


    self.onFrameSelected = function () {
        self.frameSelected(true);
        //    document.getElementById("frame-settings").scrollIntoView();
    };
    self.onSectionSelected = function (previewSection) {
        var section = null;

        for (var i = 0; i < self.sections().length; i++) {
            if (self.sections()[i].widthDescription.subscription != null && self.sections()[i].widthDescription.subscription != undefined) {
                self.sections()[i].widthDescription.subscription.dispose();
            }
            if (self.sections()[i].col() === previewSection.col) {
                section = self.sections()[i];
            }
        }
        section.widthDescription.subscribe(self.onSectionWidthChanged, this);
        section.widthDescription.extend({
            notify: 'always'
        });

        self.selectedSection(section);
        self.frameSelected(false);
        self.internalUpdateSectionWidthDescription(self.getSelectedSectionWidthDescription());
        //    document.getElementById("section-settings").scrollIntoView();
    };

    //self.sizingType = ko.observable("Brickmould");
    self.brickmouldWidthDescription = ko.observable();
    self.brickmouldHeightDescription = ko.observable();
    self.frameWidthDescription = ko.observable();
    self.frameHeightDescription = ko.observable();

    self.onSizingTypeChangedFrame = function () {
        self.sizingType("Frame");
        self.onWidthChanged();
        self.onHeightChanged();
    }
    self.onSizingTypeChangedBrickmould = function () {
        self.sizingType("Brickmould");
        self.onWidthChanged();
        self.onHeightChanged();
    }
    self.onWidthChanged = function () {
        var measurementValidationResult = self.measurementValidator.checkTextFormat(self.widthDescription());
        if (measurementValidationResult.hasErrors()) {
            alert(measurementValidationResult.errorMessage.helpfulMessage());
            self.osmWidthDescription(self.getMeasurementDescription(self.width));
            return;
        }

        var oneMeasurement = new windowStore.measurement.measurement().one();
        var frameWidthMeasurement = new windowStore.measurement.measurement();
        var brickmouldWidthMeasurement = new windowStore.measurement.measurement();

        if (self.sizingType() === "Brickmould") {
            var brickmouldDifferenceMeasurement = new windowStore.measurement.measurement();
            brickmouldDifferenceMeasurement.init(
                self.brickmouldStyle.width.sign(),
                self.brickmouldStyle.width.whole(),
                self.brickmouldStyle.width.numerator(),
                self.brickmouldStyle.width.denominator()
            );

            brickmouldWidthMeasurement.parse(self.widthDescription());

            frameWidthMeasurement = brickmouldWidthMeasurement.subtract(brickmouldDifferenceMeasurement).subtract(brickmouldDifferenceMeasurement);
        } else {
            frameWidthMeasurement.parse(self.widthDescription());

            var brickmouldDifferenceMeasurement = new windowStore.measurement.measurement();
            brickmouldDifferenceMeasurement.init(
                self.brickmouldStyle.width.sign(),
                self.brickmouldStyle.width.whole(),
                self.brickmouldStyle.width.numerator(),
                self.brickmouldStyle.width.denominator()
            );

            brickmouldWidthMeasurement = frameWidthMeasurement.add(brickmouldDifferenceMeasurement).add(brickmouldDifferenceMeasurement);
        }

        self.brickmouldWidthDescription(brickmouldWidthMeasurement.toString());
        self.frameWidthDescription(frameWidthMeasurement.toString());
        self.roWidthDescription(frameWidthMeasurement.add(oneMeasurement));

        frameWidthMeasurement.init(
            self.width.sign(),
            self.width.whole(),
            self.width.numerator(),
            self.width.denominator()
        );
        var osmWdthMeasure = frameWidthMeasurement.subtract(oneMeasurement);

        var newFrameWidthMeasurement = new windowStore.measurement.measurement();
        newFrameWidthMeasurement.parse(self.widthDescription());
        var newWidthDiffMeasurement = frameWidthMeasurement.subtract(frameWidthMeasurement);
        var newOsmWdthMeasure = newFrameWidthMeasurement.subtract(oneMeasurement);

        //
        // Frame size validation Stuff (only need to deal with width)
        //

        if (self.validator.frameWidthTooLarge(self, newFrameWidthMeasurement)) {
            var errorMessage = "A window can only be " +
                self.productLine.frameRestrictions.maxWidth.whole() +
                " " +
                self.productLine.frameRestrictions.maxWidth.numerator() +
                "/" +
                self.productLine.frameRestrictions.maxWidth.denominator() +
                " inches wide.";
            alert(errorMessage);
            self.widthDescription(self.getMeasurementDescription(self.width));
            return;
        }

        if (self.validator.frameWidthTooSmall(self, newFrameWidthMeasurement)) {
            var errorMessage = "A window must be at least " +
                self.productLine.frameRestrictions.minWidth.whole() +
                " " +
                self.productLine.frameRestrictions.minWidth.numerator() +
                "/" +
                self.productLine.frameRestrictions.minWidth.denominator() +
                " inches wide.";
            alert(errorMessage);
            self.widthDescription(self.getMeasurementDescription(self.width));
            return;
        }

        //
        // Determine the percentages of the osm width for each section and resize
        //

        var sections = self.sections();
        var secWdthPcts = [];
        var curOsmWdthDec = osmWdthMeasure.getDecimal();
        for (var i = 0; i < sections.length; i++) {
            var curSecWdthDec = new windowStore.measurement.measurement().init(
                sections[i].width.sign(),
                sections[i].width.whole(),
                sections[i].width.numerator(),
                sections[i].width.denominator()).getDecimal();

            var newOsmWdthMeasureDec = newOsmWdthMeasure.getDecimal();
            var rszdSecWdthDec = (curSecWdthDec / curOsmWdthDec) * newOsmWdthMeasure.getDecimal();
            var rszdSecWdth = new windowStore.measurement.measurement().fromDecimal(rszdSecWdthDec);

            //
            // Style Validation Stuff for resized sections
            //

            if (self.validator.sectionWidthTooLargeForStyle(sections[i].style, rszdSecWdth)) {
                var errorMessage = "A " + sections[i].style.name() + " style section can only be " +
                    sections[i].style.restrictions.maxWidth.whole() +
                    " " +
                    sections[i].style.restrictions.maxWidth.numerator() +
                    "/" +
                    sections[i].style.restrictions.maxWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                sections[i].widthDescription(self.getMeasurementDescription(sections[i].width));
                self.widthDescription(self.getMeasurementDescription(self.width));
                return;
            }

            if (self.validator.sectionWidthTooSmallForStyle(sections[i].style, rszdSecWdth)) {
                var errorMessage = "A " + sections[i].style.name() + " style section must be at least " +
                    sections[i].style.restrictions.minWidth.whole() +
                    " " +
                    sections[i].style.restrictions.minWidth.numerator() +
                    "/" +
                    sections[i].style.restrictions.minWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                sections[i].widthDescription(self.getMeasurementDescription(sections[i].width));
                self.widthDescription(self.getMeasurementDescription(self.width));
                return;
            }

            self.sections()[i].width.sign(rszdSecWdth.sign());
            self.sections()[i].width.whole(rszdSecWdth.whole());
            self.sections()[i].width.numerator(rszdSecWdth.numerator());
            self.sections()[i].width.denominator(rszdSecWdth.denominator());
        }

        for (var i = 0; i < sections.length; i++) { }

        //
        // Update Outside Measurement
        //

        var oneMeasurement = new windowStore.measurement.measurement();
        oneMeasurement.init(1, 1, 0, 1);
        var osmWidthMeasurement = newFrameWidthMeasurement.subtract(oneMeasurement);
        ko.mapping.fromJS(osmWidthMeasurement, {}, self.osmWidth);
        self.osmWidthDescription(osmWidthMeasurement.toString());

        //
        // Update Frame
        //

        ko.mapping.fromJS(newFrameWidthMeasurement, {}, self.width);
        self.internalUpdateSectionWidthDescription(self.getSelectedSectionWidthDescription());
        self.updatePreview();

        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };
    self.onHeightChanged = function () {
        var measurementValidationResult = self.measurementValidator.checkTextFormat(self.heightDescription());
        if (measurementValidationResult.hasErrors()) {
            alert(measurementValidationResult.errorMessage.helpfulMessage());
            self.heightDescription(self.getMeasurementDescription(self.height));
            return;
        }

        var oneMeasurement = new windowStore.measurement.measurement().one();
        var frameHeightMeasurement = new windowStore.measurement.measurement();
        var brickmouldHeightMeasurement = new windowStore.measurement.measurement();
        var osmWdthMeasure;

        if (self.sizingType() === "Brickmould") {
            var brickmouldDifferenceMeasurement = new windowStore.measurement.measurement();
            brickmouldDifferenceMeasurement.init(
                self.brickmouldStyle.width.sign(),
                self.brickmouldStyle.width.whole(),
                self.brickmouldStyle.width.numerator(),
                self.brickmouldStyle.width.denominator()
            );

            brickmouldHeightMeasurement.parse(self.heightDescription());

            frameHeightMeasurement = brickmouldHeightMeasurement.subtract(brickmouldDifferenceMeasurement).subtract(brickmouldDifferenceMeasurement);
        } else {
            frameHeightMeasurement.parse(self.heightDescription());

            var brickmouldDifferenceMeasurement = new windowStore.measurement.measurement();
            brickmouldDifferenceMeasurement.init(
                self.brickmouldStyle.width.sign(),
                self.brickmouldStyle.width.whole(),
                self.brickmouldStyle.width.numerator(),
                self.brickmouldStyle.width.denominator()
            );

            brickmouldHeightMeasurement = frameHeightMeasurement.add(brickmouldDifferenceMeasurement).add(brickmouldDifferenceMeasurement);
        }

        self.brickmouldHeightDescription(brickmouldHeightMeasurement.toString());
        self.frameHeightDescription(frameHeightMeasurement.toString());
        self.roHeightDescription(frameHeightMeasurement.add(oneMeasurement));

        osmWdthMeasure = frameHeightMeasurement.subtract(oneMeasurement);

        var newFrameHeightMeasurement = new windowStore.measurement.measurement();
        newFrameHeightMeasurement.parse(self.heightDescription());

        //
        // Frame Size Validation Stuff (only need to deal with height)
        //

        if (self.validator.frameHeightTooLarge(self, newFrameHeightMeasurement)) {
            var errorMessage = "A window can only be " +
                self.productLine.frameRestrictions.maxHeight.whole() +
                " " +
                self.productLine.frameRestrictions.maxHeight.numerator() +
                "/" +
                self.productLine.frameRestrictions.maxHeight.denominator() +
                " inches tall.";
            alert(errorMessage);
            self.heightDescription(self.getMeasurementDescription(self.height));
            return;
        }

        if (self.validator.frameHeightTooSmall(self, newFrameHeightMeasurement)) {
            var errorMessage = "A window must be at least " +
                self.productLine.frameRestrictions.minHeight.whole() +
                " " +
                self.productLine.frameRestrictions.minHeight.numerator() +
                "/" +
                self.productLine.frameRestrictions.minHeight.denominator() +
                " inches tall.";
            alert(errorMessage);
            self.heightDescription(self.getMeasurementDescription(self.height));
            return;
        }

        //
        // Section size validation stuff (only need to deal with height)
        //

        var oneMeasurement = new windowStore.measurement.measurement();
        oneMeasurement.init(1, 1, 0, 1);
        var newSectionHeightMeasurement = newFrameHeightMeasurement.subtract(oneMeasurement);

        var sections = self.sections();
        for (var i = 0; i < sections.length; i++) {
            if (self.validator.sectionHeightTooLargeForStyle(sections[i].style, newSectionHeightMeasurement)) {
                var errorMessage = "A " + sections[i].style.name() + " style section can only be " +
                    sections[i].style.restrictions.maxHeight.whole() +
                    " " +
                    sections[i].style.restrictions.maxHeight.numerator() +
                    "/" +
                    sections[i].style.restrictions.maxHeight.denominator() +
                    " inches tall.";
                self.heightDescription(self.getMeasurementDescription(self.height));
                alert(errorMessage);
                return;
            }

            if (self.validator.sectionHeightTooSmallForStyle(sections[i].style, newSectionHeightMeasurement)) {
                var errorMessage = "A " + sections[i].style.name() + " style section must be at least " +
                    sections[i].style.restrictions.minHeight.whole() +
                    " " +
                    sections[i].style.restrictions.minHeight.numerator() +
                    "/" +
                    sections[i].style.restrictions.minHeight.denominator() +
                    " inches tall.";
                self.heightDescription(self.getMeasurementDescription(self.height));
                alert(errorMessage);
                return;
            }
        }

        //
        // Update Outside Measurement
        //

        var osmHeightMeasurement = newFrameHeightMeasurement.subtract(oneMeasurement);
        ko.mapping.fromJS(osmHeightMeasurement, {}, self.osmHeight);
        self.osmHeightDescription(osmHeightMeasurement.toString());

        //
        // Update section heights
        //

        for (var i = 0; i < sections.length; i++) {
            ko.mapping.fromJS(newSectionHeightMeasurement.toJS(), {}, sections[i].height);
        }

        //
        // Update Frame Height
        //

        ko.mapping.fromJS(newFrameHeightMeasurement, {}, self.height);
        self.updatePreview();

        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };

    self.onSeperatorUpdated = function (seperatorPositionDiffMeasurement, leftSectionCol, rightSectionCol) {
        var sections = self.sections();
        var leftSection = null;
        var rightSection = null;

        for (var i = 0; i < sections.length; i++) {
            if (sections[i].col() === leftSectionCol) {
                leftSection = sections[i];
            }
        }

        for (var i = 0; i < sections.length; i++) {
            if (sections[i].col() === rightSectionCol) {
                rightSection = sections[i];
            }
        }

        var leftSectionWidthMeasurement = new windowStore.measurement.measurement();
        leftSectionWidthMeasurement.init(
            leftSection.width.sign(),
            leftSection.width.whole(),
            leftSection.width.numerator(),
            leftSection.width.denominator()
        );

        var rightSectionWidthMeasurement = new windowStore.measurement.measurement();
        rightSectionWidthMeasurement.init(
            rightSection.width.sign(),
            rightSection.width.whole(),
            rightSection.width.numerator(),
            rightSection.width.denominator()
        );

        leftSectionWidthMeasurement = leftSectionWidthMeasurement.add(seperatorPositionDiffMeasurement);
        rightSectionWidthMeasurement = rightSectionWidthMeasurement.subtract(seperatorPositionDiffMeasurement);

        //
        // Style Validation Stuff
        //

        // width

        if (self.validator.sectionWidthTooLargeForStyle(leftSection.style, leftSectionWidthMeasurement)) {
            var errorMessage = "A " + leftSection.style.name() + " style section can only be " +
                leftSection.style.restrictions.maxWidth.whole() +
                " " +
                leftSection.style.restrictions.maxWidth.numerator() +
                "/" +
                leftSection.style.restrictions.maxWidth.denominator() +
                " inches wide.";
            alert(errorMessage);
            return;
        }

        if (self.validator.sectionWidthTooSmallForStyle(leftSection.style, leftSectionWidthMeasurement)) {
            var errorMessage = "A " + leftSection.style.name() + " style section must be at least " +
                leftSection.style.restrictions.minWidth.whole() +
                " " +
                leftSection.style.restrictions.minWidth.numerator() +
                "/" +
                leftSection.style.restrictions.minWidth.denominator() +
                " inches wide.";
            alert(errorMessage);
            return;
        }

        if (self.validator.sectionWidthTooLargeForStyle(rightSection.style, rightSectionWidthMeasurement)) {
            var errorMessage = "A " + rightSection.style.name() + " style section can only be " +
                rightSection.style.restrictions.maxWidth.whole() +
                " " +
                rightSection.style.restrictions.maxWidth.numerator() +
                "/" +
                rightSection.style.restrictions.maxWidth.denominator() +
                " inches wide.";
            alert(errorMessage);
            return;
        }

        if (self.validator.sectionWidthTooSmallForStyle(rightSection.style, rightSectionWidthMeasurement)) {
            var errorMessage = "A " + rightSection.style.name() + " style section must be at least " +
                rightSection.style.restrictions.minWidth.whole() +
                " " +
                rightSection.style.restrictions.minWidth.numerator() +
                "/" +
                rightSection.style.restrictions.minWidth.denominator() +
                " inches wide.";
            alert(errorMessage);
            return;
        }

        ko.mapping.fromJS(leftSectionWidthMeasurement.toJS(), {}, leftSection.width);
        ko.mapping.fromJS(rightSectionWidthMeasurement.toJS(), {}, rightSection.width);

        self.internalUpdateSectionWidthDescription(self.getSelectedSectionWidthDescription());
        self.updatePreview();

        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };
    self.internalUpdateSectionWidthDescription = function (value) {
        self.sectionWidthInternalCallCount++;
        self.selectedSection().widthDescription(value);
        self.sectionWidthInternalCallCount--;
    }
    self.onSectionWidthAppliesToCurrentClick = function () {
        self.sectionWidthAppliesToCurrent = true;
        self.sectionWidthAppliesToOutside = false;
        self.sectionWidthAppliesToAll = false;
    };
    self.onSectionWidthAppliesToOutsideClick = function () {
        self.sectionWidthAppliesToCurrent = false;
        self.sectionWidthAppliesToOutside = true;
        self.sectionWidthAppliesToAll = false;
    };
    self.onSectionWidthAppliesToAllClick = function () {

        var frameWidthMeasurement = new windowStore.measurement.measurement();
        frameWidthMeasurement.parse(self.osmWidthDescription());

        var sectionCountMeasurement = new windowStore.measurement.measurement({
            sign: ko.observable(1),
            whole: ko.observable(self.sections().length),
            numerator: ko.observable(0),
            denominator: ko.observable(1)
        });

        var evenSpacingMeasurement = frameWidthMeasurement.divide(sectionCountMeasurement);

        //
        // Style Validation Stuff
        //

        // width

        var sections = self.sections();
        for (var i = 0; i < sections.length; i++) {
            var section = sections[i];
            if (self.validator.sectionWidthTooLargeForStyle(section.style, evenSpacingMeasurement)) {
                var errorMessage = "A " + section.style.name() + " style section can only be " +
                    section.style.restrictions.maxWidth.whole() + " inches wide."
                alert(errorMessage);
                return;
            }

            if (self.validator.sectionWidthTooSmallForStyle(section.style, evenSpacingMeasurement)) {
                var errorMessage = "A " + section.style.name() + " style section must be at least " +
                    section.style.restrictions.minWidth.whole() + " inches wide."
                alert(errorMessage);
                return;
            }
        }

        var section = null;
        var sections = self.sections();
        for (var i = 0; i < sections.length; i++) {
            section = sections[i]
            ko.mapping.fromJS(evenSpacingMeasurement, {}, section.width);
        };

        self.internalUpdateSectionWidthDescription(self.getSelectedSectionWidthDescription());
        self.updatePreview();

        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
        self.sectionWidthAppliesToCurrent = false;
        self.sectionWidthAppliesToOutside = false;
        self.sectionWidthAppliesToAll = true;
    };
    self.onSectionWidthChanged = function () {

        if (self.sectionWidthInternalCallCount != 0) {
            return;
        }

        //
        // if the user was somehow able to modify the width when it should be dissabled
        // should never make it this far, but just in case.
        //

        if (self.sectionWidthAppliesToOutside && self.sections().length <= 2) {
            self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
            return;
        }

        if (self.sectionWidthAppliesToAll) {
            self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
            return;
        }

        if (self.sectionWidthAppliesToCurrent && self.sections().length === 1) {
            self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
            return;
        }

        //
        // If we are dealing with the situation where there are exactly 3 sections
        // and the change should apply the the outside 2
        //

        //
        // When the center section is selected the outside sections will be adjusted
        //

        if (self.sections().length === 3 && self.selectedSection().col() === 2) {
            var newSectionWidthDescription = null;
            var isPercent = false;
            if (self.measurementValidator.isPercentage(self.selectedSection().widthDescription())) {
                var percent = parseFloat(
                    self.selectedSection().widthDescription().substring(
                        0, self.selectedSection().widthDescription().length - 1));
                newSectionWidthDescription = self.measurementFromPercentOfWidth(percent).toString();
                isPercent = true;
            } else {
                newSectionWidthDescription = self.selectedSection().widthDescription();
            }

            var measurementValidationResult =
                self.measurementValidator.checkTextFormat(newSectionWidthDescription);
            if (measurementValidationResult.hasErrors()) {
                alert(measurementValidationResult.errorMessage.helpfulMessage());
                self.internalUpdateSectionWidthDescription(
                    self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            var centerSectionWidthMeasurement = new windowStore.measurement.measurement();
            centerSectionWidthMeasurement.parse(newSectionWidthDescription);

            var osmWidthMeasurement = new windowStore.measurement.measurement();
            osmWidthMeasurement.parse(self.osmWidthDescription());

            var outsideSectionsTotalWidthMeasurement = osmWidthMeasurement.subtract(centerSectionWidthMeasurement);
            var twoMeasurement = new windowStore.measurement.measurement();
            twoMeasurement.init(1, 2, 0, 1);
            var leftSectionWidthMeasurement = outsideSectionsTotalWidthMeasurement.divide(twoMeasurement);
            var rightSectionWidthMeasurement = outsideSectionsTotalWidthMeasurement.divide(twoMeasurement);

            //
            // Style Validation Stuff
            //

            // width

            if (self.validator.sectionWidthTooLargeForStyle(self.sections()[0].style, rightSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[0].style.style.name() + " style section can only be " +
                    self.sections()[0].style.style.restrictions.maxWidth.whole() +
                    " " +
                    self.sections()[0].style.style.restrictions.maxWidth.numerator() +
                    "/" +
                    self.sections()[0].style.style.restrictions.maxWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            if (self.validator.sectionWidthTooSmallForStyle(self.sections()[0].style, rightSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[0].style.name() + " style section must be at least " +
                    self.sections()[0].style.restrictions.minWidth.whole() +
                    " " +
                    self.sections()[0].style.restrictions.minWidth.numerator() +
                    "/" +
                    self.sections()[0].style.restrictions.minWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            if (self.validator.sectionWidthTooLargeForStyle(self.sections()[1].style, leftSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[1].style.name() + " style section can only be " +
                    self.sections()[1].style.restrictions.maxWidth.whole() +
                    " " +
                    self.sections()[1].style.restrictions.maxWidth.numerator() +
                    "/" +
                    self.sections()[1].style.restrictions.maxWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            if (self.validator.sectionWidthTooSmallForStyle(self.sections()[1].style, leftSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[1].style.name() + " style section must be at least " +
                    self.sections()[1].style.restrictions.minWidth.whole() +
                    " " +
                    self.sections()[1].style.restrictions.minWidth.numerator() +
                    "/" +
                    self.sections()[1].style.restrictions.minWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            if (self.validator.sectionWidthTooLargeForStyle(self.sections()[2].style, centerSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[2].style.name() + " style section can only be " +
                    self.sections()[2].style.restrictions.maxWidth.whole() +
                    " " +
                    self.sections()[2].style.restrictions.maxWidth.numerator() +
                    "/" +
                    self.sections()[2].style.restrictions.maxWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            if (self.validator.sectionWidthTooSmallForStyle(self.sections()[2].style, centerSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[2].style.name() + " style section must be at least " +
                    self.sections()[2].style.restrictions.minWidth.whole() +
                    " " +
                    self.sections()[2].style.restrictions.minWidth.numerator() +
                    "/" +
                    self.sections()[2].style.restrictions.minWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            // Set new sizes

            ko.mapping.fromJS(rightSectionWidthMeasurement.toJS(), {}, self.sections()[0].width);
            ko.mapping.fromJS(centerSectionWidthMeasurement.toJS(), {}, self.sections()[1].width);
            ko.mapping.fromJS(leftSectionWidthMeasurement.toJS(), {}, self.sections()[2].width);

            if (isPercent) {
                self.internalUpdateSectionWidthDescription(newSectionWidthDescription);
            }
            self.updatePreview();

            var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
            self.updatePreview();

            return;
        }

        //
        // When an outside section is selected the center will be adjusted
        //

        if (self.sections().length === 3 && self.sectionWidthAppliesToOutside && (self.selectedSection().col() == 1 || self.selectedSection().col() === 3)) {
            var newSectionWidthDescription = null;
            var isPercent = false;
            if (self.measurementValidator.isPercentage(self.selectedSection().widthDescription())) {
                var percent = parseFloat(
                    self.selectedSection().widthDescription().substring(
                        0, self.selectedSection().widthDescription().length - 1));
                newSectionWidthDescription = self.measurementFromPercentOfWidth(percent).toString();
                isPercent = true;
            } else {
                newSectionWidthDescription = self.selectedSection().widthDescription();
            }

            var measurementValidationResult =
                self.measurementValidator.checkTextFormat(newSectionWidthDescription);
            if (measurementValidationResult.hasErrors()) {
                alert(measurementValidationResult.errorMessage.helpfulMessage());
                self.internalUpdateSectionWidthDescription(
                    self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            var newSectionWidth = new windowStore.measurement.measurement();
            newSectionWidth.parse(newSectionWidthDescription);

            var leftSectionWidthMeasurement = new windowStore.measurement.measurement().init(
                newSectionWidth.sign(),
                newSectionWidth.whole(),
                newSectionWidth.numerator(),
                newSectionWidth.denominator()
            );
            var rightSectionWidthMeasurement = new windowStore.measurement.measurement().init(
                newSectionWidth.sign(),
                newSectionWidth.whole(),
                newSectionWidth.numerator(),
                newSectionWidth.denominator()
            );
            var outsideSectionsTotalWidthMeasurement = leftSectionWidthMeasurement.add(rightSectionWidthMeasurement);
            var osmWidthMeasurement = new windowStore.measurement.measurement();
            osmWidthMeasurement.parse(self.osmWidthDescription());

            var centerSectionWidthMeasurement = osmWidthMeasurement.subtract(outsideSectionsTotalWidthMeasurement);

            //
            // Style Validation Stuff
            //

            // width

            if (self.validator.sectionWidthTooLargeForStyle(self.sections()[0].style, rightSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[0].style.style.name() + " style section can only be " +
                    self.sections()[0].style.style.restrictions.maxWidth.whole() +
                    " " +
                    self.sections()[0].style.style.restrictions.maxWidth.numerator() +
                    "/" +
                    self.sections()[0].style.style.restrictions.maxWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            if (self.validator.sectionWidthTooSmallForStyle(self.sections()[0].style, rightSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[0].style.name() + " style section must be at least " +
                    self.sections()[0].style.restrictions.minWidth.whole() +
                    " " +
                    self.sections()[0].style.restrictions.minWidth.numerator() +
                    "/" +
                    self.sections()[0].style.restrictions.minWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            if (self.validator.sectionWidthTooLargeForStyle(self.sections()[1].style, leftSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[1].style.name() + " style section can only be " +
                    self.sections()[1].style.restrictions.maxWidth.whole() +
                    " " +
                    self.sections()[1].style.restrictions.maxWidth.numerator() +
                    "/" +
                    self.sections()[1].style.restrictions.maxWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            if (self.validator.sectionWidthTooSmallForStyle(self.sections()[1].style, leftSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[1].style.name() + " style section must be at least " +
                    self.sections()[1].style.restrictions.minWidth.whole() +
                    " " +
                    self.sections()[1].style.restrictions.minWidth.numerator() +
                    "/" +
                    self.sections()[1].style.restrictions.minWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            if (self.validator.sectionWidthTooLargeForStyle(self.sections()[2].style, centerSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[2].style.name() + " style section can only be " +
                    self.sections()[2].style.restrictions.maxWidth.whole() +
                    " " +
                    self.sections()[2].style.restrictions.maxWidth.numerator() +
                    "/" +
                    self.sections()[2].style.restrictions.maxWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            if (self.validator.sectionWidthTooSmallForStyle(self.sections()[2].style, centerSectionWidthMeasurement)) {
                var errorMessage = "A " + self.sections()[2].style.name() + " style section must be at least " +
                    self.sections()[2].style.restrictions.minWidth.whole() +
                    " " +
                    self.sections()[2].style.restrictions.minWidth.numerator() +
                    "/" +
                    self.sections()[2].style.restrictions.minWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
                return;
            }

            // Set new sizes

            ko.mapping.fromJS(rightSectionWidthMeasurement.toJS(), {}, self.sections()[0].width);
            ko.mapping.fromJS(centerSectionWidthMeasurement.toJS(), {}, self.sections()[1].width);
            ko.mapping.fromJS(leftSectionWidthMeasurement.toJS(), {}, self.sections()[2].width);

            if (isPercent) {
                self.internalUpdateSectionWidthDescription(newSectionWidthDescription);
            }
            self.updatePreview();

            var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
            self.updatePreview();

            return;
        }

        //
        // We are dealing with a single section width change where there is >1 section
        //

        var newSectionWidthDescription = null;
        var isPercent = false;
        if (self.measurementValidator.isPercentage(self.selectedSection().widthDescription())) {
            var percent = parseFloat(
                self.selectedSection().widthDescription().substring(
                    0, self.selectedSection().widthDescription().length - 1));
            newSectionWidthDescription = self.measurementFromPercentOfWidth(percent).toString();
            isPercent = true;
        } else {
            newSectionWidthDescription = self.selectedSection().widthDescription();
        }

        var measurementValidationResult =
            self.measurementValidator.checkTextFormat(newSectionWidthDescription);
        if (measurementValidationResult.hasErrors()) {
            alert(measurementValidationResult.errorMessage.helpfulMessage());
            self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
            return;
        }

        var selectedSection = self.selectedSection();
        var sectionWidth = new windowStore.measurement.measurement();
        sectionWidth.init(
            selectedSection.width.sign(),
            selectedSection.width.whole(),
            selectedSection.width.numerator(),
            selectedSection.width.denominator()
        );

        var newSectionWidth = new windowStore.measurement.measurement();
        newSectionWidth.parse(newSectionWidthDescription);

        //
        // Get the section to the right of the selected one.  If no such thing, get the
        // one to the left of it.  If still no luck this change should not have been allowed
        // because this is a single section window.
        //

        var sections = self.sections();
        var adjacentSection = null;
        for (var i = 0; i < sections.length; i++) {
            var section = sections[i];
            if (section.col() === selectedSection.col() + 1) {
                adjacentSection = section;
            }
        }

        if (adjacentSection === null) {
            for (var i = 0; i < sections.length; i++) {
                var section = sections[i];
                if (section.col() === selectedSection.col() - 1) {
                    adjacentSection = section;
                }
            }
        }

        if (adjacentSection === null) {

            //
            // This probably only happens when converting to a single section window, the section 
            // width box should be dissabled for single section windows
            //

            return;
        }

        //
        // now use the selected section width difference to determine the adjacent section's
        // new width
        //

        var sectionWidthDifference = newSectionWidth.subtract(sectionWidth);

        var adjacentSectionWidth = new windowStore.measurement.measurement();
        adjacentSectionWidth.init(
            adjacentSection.width.sign(),
            adjacentSection.width.whole(),
            adjacentSection.width.numerator(),
            adjacentSection.width.denominator()
        );
        var newAdjacentSectionWidth = adjacentSectionWidth.subtract(sectionWidthDifference);

        //
        // Style Validation Stuff
        //

        // width

        if (self.validator.sectionWidthTooLargeForStyle(self.selectedSection().style, newSectionWidth)) {
            var errorMessage = "A " + self.selectedSection().style.name() + " style section can only be " +
                self.selectedSection().style.restrictions.maxWidth.whole() +
                " " +
                self.selectedSection().style.restrictions.maxWidth.numerator() +
                "/" +
                self.selectedSection().style.restrictions.maxWidth.denominator() +
                " inches wide.";
            alert(errorMessage);
            self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
            return;
        }

        if (self.validator.sectionWidthTooSmallForStyle(self.selectedSection().style, newSectionWidth)) {
            var errorMessage = "A " + self.selectedSection().style.name() + " style section must be at least " +
                self.selectedSection().style.restrictions.minWidth.whole() +
                " " +
                self.selectedSection().style.restrictions.minWidth.numerator() +
                "/" +
                self.selectedSection().style.restrictions.minWidth.denominator() +
                " inches wide.";
            alert(errorMessage);
            self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
            return;
        }

        if (self.validator.sectionWidthTooLargeForStyle(adjacentSection.style, newAdjacentSectionWidth)) {
            var errorMessage = "A " + adjacentSection.style.name() + " style section can only be " +
                adjacentSection.style.restrictions.maxWidth.whole() +
                " " +
                adjacentSection.style.restrictions.maxWidth.numerator() +
                "/" +
                adjacentSection.style.restrictions.maxWidth.denominator() +
                " inches wide.";
            alert(errorMessage);
            self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
            return;
        }

        if (self.validator.sectionWidthTooSmallForStyle(adjacentSection.style, newAdjacentSectionWidth)) {
            var errorMessage = "A " + adjacentSection.style.name() + " style section must be at least " +
                adjacentSection.style.restrictions.minWidth.whole() +
                " " +
                adjacentSection.style.restrictions.minWidth.numerator() +
                "/" +
                adjacentSection.style.restrictions.minWidth.denominator() +
                " inches wide.";
            alert(errorMessage);
            self.internalUpdateSectionWidthDescription(self.getMeasurementDescription(self.selectedSection().width));
            return;
        }

        // Set new sizes

        ko.mapping.fromJS(newSectionWidth.toJS(), {}, self.selectedSection().width);
        ko.mapping.fromJS(newAdjacentSectionWidth.toJS(), {}, adjacentSection.width);
        if (isPercent) {
            self.internalUpdateSectionWidthDescription(newSectionWidthDescription);
        }
        self.updatePreview();

        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    }


    self.onStyleAppliesToCurrentClick = function () {
        self.styleAppliesToCurrent = true;
        self.styleAppliesToOutside = false;
        self.styleAppliesToAll = false;
    };
    self.onStyleAppliesToOutsideClick = function () {
        self.styleAppliesToCurrent = false;
        self.styleAppliesToOutside = true;
        self.styleAppliesToAll = false;
    };
    self.onStyleAppliesToAllClick = function () {
        self.styleAppliesToCurrent = false;
        self.styleAppliesToOutside = false;
        self.styleAppliesToAll = true;
    };
    self.onStyleSelected = function (style) {

        if (self.styleAppliesToOutside && self.sections().length === 3) {
            {
                var selectedSetionWidth = new windowStore.measurement.measurement();
                selectedSetionWidth.init(
                    self.sections()[0].width.sign(),
                    self.sections()[0].width.whole(),
                    self.sections()[0].width.numerator(),
                    self.sections()[0].width.denominator()
                );

                var selectedSetionHeight = new windowStore.measurement.measurement();
                selectedSetionHeight.init(
                    self.sections()[0].height.sign(),
                    self.sections()[0].height.whole(),
                    self.sections()[0].height.numerator(),
                    self.sections()[0].height.denominator()
                );

                if (self.validator.sectionWidthTooLargeForStyle(style, selectedSetionWidth)) {
                    var errorMessage = "A " + style.name() + " style section can only be " +
                        style.restrictions.maxWidth.whole() +
                        " " +
                        style.restrictions.maxWidth.numerator() +
                        "/" +
                        style.restrictions.maxWidth.denominator() +
                        " inches wide.";
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionWidthTooSmallForStyle(style, selectedSetionWidth)) {
                    var errorMessage = "A " + style.name() + " style section must be at least " +
                        style.restrictions.minWidth.whole() +
                        " " +
                        style.restrictions.minWidth.numerator() +
                        "/" +
                        style.restrictions.minWidth.denominator() +
                        " inches wide.";
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionHeightTooLargeForStyle(style, selectedSetionHeight)) {
                    var errorMessage = "A " + style.name() + " style section can only be " +
                        style.restrictions.maxHeight.whole() +
                        " " +
                        style.restrictions.maxHeight.numerator() +
                        "/" +
                        style.restrictions.maxHeight.denominator() +
                        " inches tall.";
                    self.heightDescription(self.getMeasurementDescription(self.height));
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionHeightTooSmallForStyle(style, selectedSetionHeight)) {
                    var errorMessage = "A " + style.name() + " style section must be at least " +
                        style.restrictions.minHeight.whole() +
                        " " +
                        style.restrictions.minHeight.numerator() +
                        "/" +
                        style.restrictions.minHeight.denominator() +
                        " inches tall.";
                    self.heightDescription(self.getMeasurementDescription(self.height));
                    alert(errorMessage);
                    return;
                }

                sectionHeightToo

                //
                // Crank validation stuff
                //

                var crankName = "Regular";
                for (var j = 0; j < self.sections().length; j++) {
                    if (self.sections()[j].crank.name() !== "None") {
                        crankName = self.sections()[j].crank.name();
                    }
                }

                if (!style.hasCrank()) {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].crank);
                } else {
                    ko.mapping.fromJS({
                        name: crankName
                    }, {}, self.sections()[0].crank);
                }

                ko.mapping.fromJS(style, {}, self.sections()[0].style);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }

            {
                var selectedSetionWidth = new windowStore.measurement.measurement();
                selectedSetionWidth.init(
                    self.sections()[2].width.sign(),
                    self.sections()[2].width.whole(),
                    self.sections()[2].width.numerator(),
                    self.sections()[2].width.denominator()
                );

                var selectedSetionHeight = new windowStore.measurement.measurement();
                selectedSetionHeight.init(
                    self.sections()[2].height.sign(),
                    self.sections()[2].height.whole(),
                    self.sections()[2].height.numerator(),
                    self.sections()[2].height.denominator()
                );

                if (self.validator.sectionWidthTooLargeForStyle(style, selectedSetionWidth)) {
                    var errorMessage = "A " + style.name() + " style section can only be " +
                        style.restrictions.maxWidth.whole() +
                        " " +
                        style.restrictions.maxWidth.numerator() +
                        "/" +
                        style.restrictions.maxWidth.denominator() +
                        " inches wide.";
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionWidthTooSmallForStyle(style, selectedSetionWidth)) {
                    var errorMessage = "A " + style.name() + " style section must be at least " +
                        style.restrictions.minWidth.whole() +
                        " " +
                        style.restrictions.minWidth.numerator() +
                        "/" +
                        style.restrictions.minWidth.denominator() +
                        " inches wide.";
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionHeightTooLargeForStyle(style, selectedSetionHeight)) {
                    var errorMessage = "A " + style.name() + " style section can only be " +
                        style.restrictions.maxHeight.whole() +
                        " " +
                        style.restrictions.maxHeight.numerator() +
                        "/" +
                        style.restrictions.maxHeight.denominator() +
                        " inches tall.";
                    self.heightDescription(self.getMeasurementDescription(self.height));
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionHeightTooSmallForStyle(style, selectedSetionHeight)) {
                    var errorMessage = "A " + style.name() + " style section must be at least " +
                        style.restrictions.minHeight.whole() +
                        " " +
                        style.restrictions.minHeight.numerator() +
                        "/" +
                        style.restrictions.minHeight.denominator() +
                        " inches tall.";
                    self.heightDescription(self.getMeasurementDescription(self.height));
                    alert(errorMessage);
                    return;
                }

                //
                // Crank validation stuff
                //

                var crankName = "Regular";
                for (var j = 0; j < self.sections().length; j++) {
                    if (self.sections()[j].crank.name() !== "None") {
                        crankName = self.sections()[j].crank.name();
                    }
                }

                if (!style.hasCrank()) {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[2].crank);
                } else {
                    ko.mapping.fromJS({
                        name: crankName
                    }, {}, self.sections()[2].crank);
                }

                ko.mapping.fromJS(style, {}, self.sections()[2].style);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }
            return;
        }

        if (self.styleAppliesToOutside && self.sections().length <= 2) {
            for (var i = 0; i < self.sections().length; i++) {
                var selectedSetionWidth = new windowStore.measurement.measurement();
                selectedSetionWidth.init(
                    self.sections()[i].width.sign(),
                    self.sections()[i].width.whole(),
                    self.sections()[i].width.numerator(),
                    self.sections()[i].width.denominator()
                );

                var selectedSetionHeight = new windowStore.measurement.measurement();
                selectedSetionHeight.init(
                    self.sections()[i].height.sign(),
                    self.sections()[i].height.whole(),
                    self.sections()[i].height.numerator(),
                    self.sections()[i].height.denominator()
                );

                if (self.validator.sectionWidthTooLargeForStyle(style, selectedSetionWidth)) {
                    var errorMessage = "A " + style.name() + " style section can only be " +
                        style.restrictions.maxWidth.whole() +
                        " " +
                        style.restrictions.maxWidth.numerator() +
                        "/" +
                        style.restrictions.maxWidth.denominator() +
                        " inches wide.";
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionWidthTooSmallForStyle(style, selectedSetionWidth)) {
                    var errorMessage = "A " + style.name() + " style section must be at least " +
                        style.restrictions.minWidth.whole() +
                        " " +
                        style.restrictions.minWidth.numerator() +
                        "/" +
                        style.restrictions.minWidth.denominator() +
                        " inches wide.";
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionHeightTooLargeForStyle(style, selectedSetionHeight)) {
                    var errorMessage = "A " + style.name() + " style section can only be " +
                        style.restrictions.maxHeight.whole() +
                        " " +
                        style.restrictions.maxHeight.numerator() +
                        "/" +
                        style.restrictions.maxHeight.denominator() +
                        " inches tall.";
                    self.heightDescription(self.getMeasurementDescription(self.height));
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionHeightTooSmallForStyle(style, selectedSetionHeight)) {
                    var errorMessage = "A " + style.name() + " style section must be at least " +
                        restrictions.minHeight.whole() +
                        " " +
                        style.restrictions.minHeight.numerator() +
                        "/" +
                        style.restrictions.minHeight.denominator() +
                        " inches tall.";
                    self.heightDescription(self.getMeasurementDescription(self.height));
                    alert(errorMessage);
                    return;
                }

                //
                // Crank validation stuff
                //

                var crankName = "Regular";
                for (var j = 0; j < self.sections().length; j++) {
                    if (self.sections()[j].crank.name() !== "None") {
                        crankName = self.sections()[j].crank.name();
                    }
                }

                if (!style.hasCrank()) {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[i].crank);
                } else {
                    ko.mapping.fromJS({
                        name: crankName
                    }, {}, self.sections()[i].crank);
                }

                ko.mapping.fromJS(style, {}, self.sections()[i].style);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }
            return;
        }

        if (self.styleAppliesToAll) {
            for (var i = 0; i < self.sections().length; i++) {
                var selectedSetionWidth = new windowStore.measurement.measurement();
                selectedSetionWidth.init(
                    self.sections()[i].width.sign(),
                    self.sections()[i].width.whole(),
                    self.sections()[i].width.numerator(),
                    self.sections()[i].width.denominator()
                );

                var selectedSetionHeight = new windowStore.measurement.measurement();
                selectedSetionHeight.init(
                    self.sections()[i].height.sign(),
                    self.sections()[i].height.whole(),
                    self.sections()[i].height.numerator(),
                    self.sections()[i].height.denominator()
                );

                if (self.validator.sectionWidthTooLargeForStyle(style, selectedSetionWidth)) {
                    var errorMessage = "A " + style.name() + " style section can only be " +
                        style.restrictions.maxWidth.whole() +
                        " " +
                        style.restrictions.maxWidth.numerator() +
                        "/" +
                        style.restrictions.maxWidth.denominator() +
                        " inches wide.";
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionWidthTooSmallForStyle(style, selectedSetionWidth)) {
                    var errorMessage = "A " + style.name() + " style section must be at least " +
                        style.restrictions.minWidth.whole() +
                        " " +
                        style.restrictions.minWidth.numerator() +
                        "/" +
                        style.restrictions.minWidth.denominator() +
                        " inches wide.";
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionHeightTooLargeForStyle(style, selectedSetionHeight)) {
                    var errorMessage = "A " + style.name() + " style section can only be " +
                        style.restrictions.maxHeight.whole() +
                        " " +
                        style.restrictions.maxHeight.numerator() +
                        "/" +
                        style.restrictions.maxHeight.denominator() +
                        " inches tall.";
                    self.heightDescription(self.getMeasurementDescription(self.height));
                    alert(errorMessage);
                    return;
                }

                if (self.validator.sectionHeightTooSmallForStyle(style, selectedSetionHeight)) {
                    var errorMessage = "A " + style.name() + " style section must be at least " +
                        style.restrictions.minHeight.whole() +
                        " " +
                        style.restrictions.minHeight.numerator() +
                        "/" +
                        style.restrictions.minHeight.denominator() +
                        " inches tall.";
                    self.heightDescription(self.getMeasurementDescription(self.height));
                    alert(errorMessage);
                    return;
                }

                //
                // Crank validation stuff
                //

                var crankName = "Regular";
                for (var j = 0; j < self.sections().length; j++) {
                    if (self.sections()[j].crank.name() !== "None") {
                        crankName = self.sections()[j].crank.name();
                    }
                }

                if (!style.hasCrank()) {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[i].crank);
                } else {
                    ko.mapping.fromJS({
                        name: crankName
                    }, {}, self.sections()[i].crank);
                }


                ko.mapping.fromJS(style, {}, self.sections()[i].style);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }
            return;
        }

        {
            //
            // Style Validation Stuff
            //

            // width

            var selectedSetionWidth = new windowStore.measurement.measurement();
            selectedSetionWidth.init(
                self.selectedSection().width.sign(),
                self.selectedSection().width.whole(),
                self.selectedSection().width.numerator(),
                self.selectedSection().width.denominator()
            );

            var selectedSetionHeight = new windowStore.measurement.measurement();
            selectedSetionHeight.init(
                self.selectedSection().height.sign(),
                self.selectedSection().height.whole(),
                self.selectedSection().height.numerator(),
                self.selectedSection().height.denominator()
            );

            if (self.validator.sectionWidthTooLargeForStyle(style, selectedSetionWidth)) {
                var errorMessage = "A " + style.name() + " style section can only be " +
                    style.restrictions.maxWidth.whole() +
                    " " +
                    style.restrictions.maxWidth.numerator() +
                    "/" +
                    style.restrictions.maxWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                return;
            }

            if (self.validator.sectionWidthTooSmallForStyle(style, selectedSetionWidth)) {
                var errorMessage = "A " + style.name() + " style section must be at least " +
                    style.restrictions.minWidth.whole() +
                    " " +
                    style.restrictions.minWidth.numerator() +
                    "/" +
                    style.restrictions.minWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                return;
            }

            if (self.validator.sectionHeightTooLargeForStyle(style, selectedSetionHeight)) {
                var errorMessage = "A " + style.name() + " style section can only be " +
                    style.restrictions.maxHeight.whole() +
                    " " +
                    style.restrictions.maxHeight.numerator() +
                    "/" +
                    style.restrictions.maxHeight.denominator() +
                    " inches tall.";
                self.heightDescription(self.getMeasurementDescription(self.height));
                alert(errorMessage);
                return;
            }

            if (self.validator.sectionHeightTooSmallForStyle(style, selectedSetionHeight)) {
                var errorMessage = "A " + style.name() + " style section must be at least " +
                    style.restrictions.minHeight.whole() +
                    " " +
                    style.restrictions.minHeight.numerator() +
                    "/" +
                    style.restrictions.minHeight.denominator() +
                    " inches tall.";
                self.heightDescription(self.getMeasurementDescription(self.height));
                alert(errorMessage);
                return;
            }

            //
            // Crank validation stuff
            //

            var crankName = "Regular";
            for (var i = 0; i < self.sections().length; i++) {
                if (self.sections()[i].crank.name() !== "None") {
                    crankName = self.sections()[i].crank.name();
                }
            }

            if (!style.hasCrank()) {
                ko.mapping.fromJS({
                    name: "None"
                }, {}, self.sections()[0].crank);
            } else {
                ko.mapping.fromJS({
                    name: crankName
                }, {}, self.sections()[0].crank);
            }

            ko.mapping.fromJS(style, {}, self.selectedSection().style);
            var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
            self.updatePreview();
        }
    };
    self.onCrankSelected = function (crank) {
        for (var i = 0; i < self.sections().length; i++) {
            if (self.sections()[i].style.hasCrank()) {
                ko.mapping.fromJS(crank, {}, self.sections()[i].crank);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }
        }
    };

    self.onGrilleAppliesToCurrentClick = function () {
        self.grilleAppliesToCurrent = true;
        self.grilleAppliesToOutside = false;
        self.grilleAppliesToAll = false;
    };
    self.onGrilleAppliesToOutsideClick = function () {
        self.grilleAppliesToCurrent = false;
        self.grilleAppliesToOutside = true;
        self.grilleAppliesToAll = false;
    };
    self.onGrilleAppliesToAllClick = function () {
        self.grilleAppliesToCurrent = false;
        self.grilleAppliesToOutside = false;
        self.grilleAppliesToAll = true;
    };
    self.onGrillePatternSelected = function (grillePattern) {
        if (self.grilleAppliesToOutside && self.sections().length === 3) {
            {
                if (grillePattern.name() === "None") {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].grilleColor);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].grilleSize);
                } else {
                    ko.mapping.fromJS(self.productLine.grilleColors()[0], {}, self.sections()[0].grilleColor);
                    ko.mapping.fromJS(self.productLine.grilleSizes()[0], {}, self.sections()[0].grilleSize);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].sdlPattern);
                }

                ko.mapping.fromJS(grillePattern, {}, self.sections()[0].grillePattern);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }

            {
                if (grillePattern.name() === "None") {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[2].grilleColor);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[2].grilleSize);
                } else {
                    ko.mapping.fromJS(self.productLine.grilleColors()[0], {}, self.sections()[2].grilleColor);
                    ko.mapping.fromJS(self.productLine.grilleSizes()[0], {}, self.sections()[2].grilleSize);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[2].sdlPattern);
                }

                ko.mapping.fromJS(grillePattern, {}, self.sections()[2].grillePattern);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }
            return;
        }

        if (self.grilleAppliesToOutside && self.sections().length <= 2) {
            {
                if (grillePattern.name() === "None") {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].grilleColor);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].grilleSize);
                } else {
                    ko.mapping.fromJS(self.productLine.grilleColors()[0], {}, self.sections()[0].grilleColor);
                    ko.mapping.fromJS(self.productLine.grilleSizes()[0], {}, self.sections()[0].grilleSize);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].sdlPattern);
                }

                ko.mapping.fromJS(grillePattern, {}, self.sections()[0].grillePattern);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }

            {
                if (grillePattern.name() === "None") {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[1].grilleColor);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[1].grilleSize);
                } else {
                    ko.mapping.fromJS(self.productLine.grilleColors()[0], {}, self.sections()[1].grilleColor);
                    ko.mapping.fromJS(self.productLine.grilleSizes()[0], {}, self.sections()[1].grilleSize);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[1].sdlPattern);
                }

                ko.mapping.fromJS(grillePattern, {}, self.sections()[1].grillePattern);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }
            return;
        }

        if (self.grilleAppliesToAll) {
            for (var i = 0; i < self.sections().length; i++) {
                if (grillePattern.name() === "None") {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[i].grilleColor);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[i].grilleSize);
                } else {
                    ko.mapping.fromJS(self.productLine.grilleColors()[0], {}, self.sections()[i].grilleColor);
                    ko.mapping.fromJS(self.productLine.grilleSizes()[0], {}, self.sections()[i].grilleSize);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[i].sdlPattern);
                }

                ko.mapping.fromJS(grillePattern, {}, self.sections()[i].grillePattern);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }
            return;
        }

        {
            if (grillePattern.name() === "None") {
                ko.mapping.fromJS({
                    name: "None"
                }, {}, self.selectedSection().grilleColor);
                ko.mapping.fromJS({
                    name: "None"
                }, {}, self.selectedSection().grilleSize);
            } else {
                ko.mapping.fromJS(self.productLine.grilleColors()[0], {}, self.selectedSection().grilleColor);
                ko.mapping.fromJS(self.productLine.grilleSizes()[0], {}, self.selectedSection().grilleSize);
                ko.mapping.fromJS({
                    name: "None"
                }, {}, self.selectedSection().sdlPattern);
            }

            ko.mapping.fromJS(grillePattern, {}, self.selectedSection().grillePattern);
            var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
            self.updatePreview();
        }
    };
    self.onGrilleColorSelected = function (grilleColor) {
        for (var i = 0; i < self.sections().length; i++) {
            ko.mapping.fromJS(grilleColor, {}, self.sections()[i].grilleColor);
        }
        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };
    self.onGrilleSizeSelected = function (grilleSize) {
        for (var i = 0; i < self.sections().length; i++) {
            ko.mapping.fromJS(grilleColor, {}, self.sections()[i].grilleColor);
        }
        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };

    self.onSdlAppliesToCurrentClick = function () {
        self.sdlAppliesToCurrent = true;
        self.sdlAppliesToOutside = false;
        self.sdlAppliesToAll = false;
    };
    self.onSdlAppliesToOutsideClick = function () {
        self.sdlAppliesToCurrent = false;
        self.sdlAppliesToOutside = true;
        self.sdlAppliesToAll = false;
    };
    self.onSdlAppliesToAllClick = function () {
        self.sdlAppliesToCurrent = false;
        self.sdlAppliesToOutside = false;
        self.sdlAppliesToAll = true;
    };
    self.onSdlPatternSelected = function (sdlPattern) {

        if (self.sdlAppliesToOutside && self.sections().length === 3) {
            {
                if (sdlPattern.name() === "None") {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].sdlColor);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].sdlSize);
                } else {
                    ko.mapping.fromJS(self.productLine.sdlColors()[0], {}, self.sections()[0].sdlColor);
                    ko.mapping.fromJS(self.productLine.sdlSizes()[0], {}, self.sections()[0].sdlSize);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].grillePattern);
                }

                ko.mapping.fromJS(sdlPattern, {}, self.sections()[0].sdlPattern);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }

            {
                if (sdlPattern.name() === "None") {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[2].sdlColor);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[2].sdlSize);
                } else {
                    ko.mapping.fromJS(self.productLine.sdlColors()[0], {}, self.sections()[2].sdlColor);
                    ko.mapping.fromJS(self.productLine.sdlSizes()[0], {}, self.sections()[2].sdlSize);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[2].grillePattern);
                }

                ko.mapping.fromJS(sdlPattern, {}, self.sections()[2].sdlPattern);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }
            return;
        }

        if (self.sdlAppliesToOutside && self.sections().length <= 2) {
            {
                if (sdlPattern.name() === "None") {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].sdlColor);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].sdlSize);
                } else {
                    ko.mapping.fromJS(self.productLine.sdlColors()[0], {}, self.sections()[0].sdlColor);
                    ko.mapping.fromJS(self.productLine.sdlSizes()[0], {}, self.sections()[0].sdlSize);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[0].grillePattern);
                }

                ko.mapping.fromJS(sdlPattern, {}, self.sections()[0].sdlPattern);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }

            {
                if (sdlPattern.name() === "None") {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[1].sdlColor);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[1].sdlSize);
                } else {
                    ko.mapping.fromJS(self.productLine.sdlColors()[0], {}, self.sections()[1].sdlColor);
                    ko.mapping.fromJS(self.productLine.sdlSizes()[0], {}, self.sections()[1].sdlSize);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[1].grillePattern);
                }

                ko.mapping.fromJS(sdlPattern, {}, self.sections()[1].sdlPattern);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }
            return;
        }

        if (self.sdlAppliesToAll) {
            for (var i = 0; i < self.sections().length; i++) {
                if (sdlPattern.name() === "None") {
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[i].sdlColor);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[i].sdlSize);
                } else {
                    ko.mapping.fromJS(self.productLine.sdlColors()[0], {}, self.sections()[i].sdlColor);
                    ko.mapping.fromJS(self.productLine.sdlSizes()[0], {}, self.sections()[i].sdlSize);
                    ko.mapping.fromJS({
                        name: "None"
                    }, {}, self.sections()[i].grillePattern);
                }

                ko.mapping.fromJS(sdlPattern, {}, self.sections()[i].sdlPattern);
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            }
            return;
        }

        {
            if (sdlPattern.name() === "None") {
                ko.mapping.fromJS({
                    name: "None"
                }, {}, self.selectedSection().sdlColor);
                ko.mapping.fromJS({
                    name: "None"
                }, {}, self.selectedSection().sdlSize);
            } else {
                ko.mapping.fromJS(self.productLine.sdlColors()[0], {}, self.selectedSection().sdlColor);
                ko.mapping.fromJS(self.productLine.sdlSizes()[0], {}, self.selectedSection().sdlSize);
                ko.mapping.fromJS({
                    name: "None"
                }, {}, self.selectedSection().grillePattern);
            }

            ko.mapping.fromJS(sdlPattern, {}, self.selectedSection().sdlPattern);
            var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
            self.updatePreview();
        }
    };
    self.onSdlColorSelected = function (sdlColor) {
        for (var i = 0; i < self.sections().length; i++) {
            ko.mapping.fromJS(sdlColor, {}, self.sections()[i].sdlColor);
        }
        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };
    self.onSdlSizeSelected = function (sdlSize) {
        for (var i = 0; i < self.sections().length; i++) {
            ko.mapping.fromJS(sdlSize, {}, self.sections()[i].sdlSize);
        }
        var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
        self.updatePreview();
    };


    self.onOneSectionButtonClick = function () {
        if (self.sections().length === 1) {
            return;
        }

        var url = "OrderItem/SectionTemplate/" + self.productLine.sectionTemplateName();
        $.ajax({
            cache: false,
            type: "GET",
            dataType: "json",
            url: url
        })
            .done(function (data, textStatus, jqXHR) {
                self.updateSectionCount(1, data);
                self.selectedSection(self.sections()[0]);
                self.internalUpdateSectionWidthDescription(self.getSelectedSectionWidthDescription());
                self.updatePreview();
                var sections = self.sections();
                for (var i = 0; i < sections.length; i++) {
                    var section = sections[i];
                }
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            });
    };
    self.onTwoSectionButtonClick = function () {
        if (self.sections().length === 2) {
            return;
        }

        var url = "OrderItem/SectionTemplate/" + self.productLine.sectionTemplateName();
        $.ajax({
            cache: false,
            type: "GET",
            dataType: "json",
            url: url
        })
            .done(function (data, textStatus, jqXHR) {
                self.updateSectionCount(2, data);
                self.selectedSection(self.sections()[0]);
                self.internalUpdateSectionWidthDescription(self.getSelectedSectionWidthDescription());
                self.updatePreview();
                var sections = self.sections();
                for (var i = 0; i < sections.length; i++) {
                    var section = sections[i];
                }
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            });
    };
    self.onThreeSectionButtonClick = function () {
        if (self.sections().length === 3) {
            return;
        }

        var url = "OrderItem/SectionTemplate/" + self.productLine.sectionTemplateName();
        $.ajax({
            cache: false,
            type: "GET",
            dataType: "json",
            url: url
        })
            .done(function (data, textStatus, jqXHR) {
                self.updateSectionCount(3, data);
                self.selectedSection(self.sections()[0]);
                self.internalUpdateSectionWidthDescription(self.getSelectedSectionWidthDescription());
                self.updatePreview();
                var sections = self.sections();
                for (var i = 0; i < sections.length; i++) {
                    var section = sections[i];
                }
                var priceViewModelField = self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);
                self.updatePreview();
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            });
    };



    self.loadItem = function (templateName, sessionId, success) {
        self.orderId = sessionId;
        var url = "OrderItem/GetOverview/" + sessionId;
        return $.ajax({
            cache: false,
            type: "GET",
            dataType: "json",
            url: url
        })
            .done(function (data, textStatus, jqXHR) {
                // The template JSON is legacy demo data and still carries stale root IDs.
                // Strip those fields so the live session id stays authoritative.
                delete data.id;
                delete data.orderId;
                ko.mapping.fromJS(data, {}, self);
                self.orderId = sessionId;

                for (var i = 0; i < self.productLine.cranks().length; i++) {
                    if (self.productLine.cranks()[i].name() === "None") {
                        self.productLine.cranks.splice(i, 1);
                    }
                }

                self.configurePreview();

                self.setSelectedSectionReference();

                self.widthDescription.subscribe(self.onWidthChanged, this);
                self.widthDescription.extend({
                    notify: 'always'
                });

                self.heightDescription.subscribe(self.onHeightChanged, this);
                self.heightDescription.extend({
                    notify: 'always'
                });

                // Price calculation
                // DISSABLE moz TO GET RID OF SOME CONFUSING ERRORS HERE
                self.priceCalc.calculatePrice(ko.mapping.toJS(self), self.price);

                if (success)
                    success(self);
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            });
    };
    self.setSelectedSectionReference = function () {

        //
        // Selected section is passed from the server and so does not point to
        // an object in the sections array, but we want it to
        //

        var sections = self.sections();

        for (var i = 0; i < sections.length; i++) {
            var section = sections[i];
            if (self.selectedSection.col() === section.col()) {
                section.widthDescription.subscription = section.widthDescription.subscribe(self.onSectionWidthChanged, this);
                section.widthDescription.extend({
                    notify: 'always'
                });
                self.selectedSection = ko.observable(section);
                break;
            }
        }
        return null;
    };
    self.getSelectedSectionWidthDescription = function () {
        var widthDescription = self.selectedSection().width.sign() == 1 ? "" : "-";
        widthDescription += self.selectedSection().width.whole() + " ";
        widthDescription += self.selectedSection().width.numerator() + "/";
        widthDescription += self.selectedSection().width.denominator();
        return widthDescription;
    };
    self.getMeasurementDescription = function (measurement) {
        var measurementDescription = measurement.sign() == 1 ? "" : "-";
        measurementDescription += measurement.whole() + " ";
        measurementDescription += measurement.numerator() + "/";
        measurementDescription += measurement.denominator();
        return measurementDescription;
    };
    self.configurePreview = function () {
        self.preview = new windowStore.order.item.preview.viewModel.WindowPreview();
        self.preview.sectionSelectedCallback = self.onSectionSelected;
        self.updatePreview();
    };
    self.updatePreview = function () {

        // Calculate the new scale

        var scale = 0;
        var drawingSurfaceWidth = $("#preview-svg").outerWidth() - 10;
        var drawingSurfaceHeight = $("#preview-svg").outerHeight() - 10;

        var widthMeasurement = new windowStore.measurement.measurement(self.osmWidth);
        var widthDecimal = widthMeasurement.getDecimal();
        var heightMeasurement = new windowStore.measurement.measurement(self.osmHeight);
        var heightDecimal = heightMeasurement.getDecimal();

        // Add brickmould width

        var brickmouldWidth = new windowStore.measurement.measurement(self.brickmouldStyle.width);
        var brickmouldWidthDecimal = brickmouldWidth.getDecimal();
        widthDecimal += (2 * brickmouldWidthDecimal);
        heightDecimal += (2 * brickmouldWidthDecimal);

        if ((widthDecimal !== 0) || (heightDecimal !== 0)) {
            scale = drawingSurfaceWidth / widthDecimal;
            scale -= 1;

            self.preview.scale(scale);

            // Calculate new x and y positions

            var scaledWidth = widthDecimal * scale;
            var widthSpace = drawingSurfaceWidth - scaledWidth;
            self.preview.x((widthSpace / 2) + 5 + (brickmouldWidthDecimal * scale));

            var scaledHeight = heightDecimal * scale;
            var heightSpace = drawingSurfaceHeight - scaledHeight;
            self.preview.y((heightSpace / 2) + 5 + (brickmouldWidthDecimal * scale));

            if ((scale * heightDecimal) >= drawingSurfaceHeight) {
                scale = drawingSurfaceHeight / heightDecimal;
                scale -= 1;

                self.preview.scale(scale);

                // Calculate new x and y positions

                scaledWidth = widthDecimal * scale;
                widthSpace = drawingSurfaceWidth - scaledWidth;
                self.preview.x((widthSpace / 2) + 5 + (brickmouldWidthDecimal * scale));

                scaledHeight = heightDecimal * scale;
                heightSpace = drawingSurfaceHeight - scaledHeight;
                self.preview.y((heightSpace / 2) + 5 + (brickmouldWidthDecimal * scale));
            }
        }

        self.preview.update(ko.mapping.toJS(self));
    };
    self.measurementFromPercentOfWidth = function (percent) {
        var res = new windowStore.measurement.measurement();
        var widthMeasurement = new windowStore.measurement.measurement();
        widthMeasurement.init(
            self.width.sign(),
            self.width.whole(),
            self.width.numerator(),
            self.width.denominator()
        );

        var percentAsMeasurement = new windowStore.measurement.measurement();
        percentAsMeasurement.numerator(percent);
        percentAsMeasurement.denominator(100);

        return widthMeasurement.multiply(percentAsMeasurement);
    };
    self.updateSectionCount = function (newSectionCount, newJsSection) {
        var sections = self.sections();

        //
        // Exit if no change is nessissary
        //

        if (sections.length === newSectionCount) {
            return;
        }

        var osmWidthMeasurement = new windowStore.measurement.measurement();
        osmWidthMeasurement.parse(self.osmWidthDescription());

        var sectionCountMeasurement = new windowStore.measurement.measurement({
            sign: ko.observable(1),
            whole: ko.observable(newSectionCount),
            numerator: ko.observable(0),
            denominator: ko.observable(1)
        });

        var newSectionWidthMeasurement = osmWidthMeasurement.divide(sectionCountMeasurement);

        //
        // Style Validation Stuff
        //

        // Existing sections

        var numberOfSectionsToCheck = Math.min(sections.length, newSectionCount);

        for (var i = 0; i < numberOfSectionsToCheck; i++) {
            var section = sections[i];
            if (self.validator.sectionWidthTooLargeForStyle(section.style, newSectionWidthMeasurement)) {
                var errorMessage = "A " + section.style.name() + " style section can only be " +
                    section.style.restrictions.maxWidth.whole() +
                    " " +
                    section.style.restrictions.maxWidth.numerator() +
                    "/" +
                    section.style.restrictions.maxWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                return;
            }

            if (self.validator.sectionWidthTooSmallForStyle(section.style, newSectionWidthMeasurement)) {
                var errorMessage = "A " + section.style.name() + " style section must be at least " +
                    section.style.restrictions.minWidth.whole() +
                    " " +
                    section.style.restrictions.minWidth.numerator() +
                    "/" +
                    section.style.restrictions.minWidth.denominator() +
                    " inches wide.";
                alert(errorMessage);
                return;
            }
        }

        if (sections.length <= newSectionCount) {
            var additionalSectionsCount = newSectionCount - self.sections().length;

            //
            // Adding new sections, keeping all old ones
            //

            for (var i = 0; i <= additionalSectionsCount; i++) {
                var greatestColSection = null;

                for (var i = 0; i < sections.length; i++) {
                    if (i === 0 || sections[i].col() > greatestColSection.col()) {
                        greatestColSection = sections[i];
                    }
                };

                newJsSection.col = greatestColSection.col() + 1;
                var newKoSection = {};
                ko.mapping.fromJS(newJsSection, {}, newKoSection);
                sections.push(newKoSection);
            }
        } else {

            //
            // just removing the last ones
            //

            var sectionsToRemoveCount = self.sections().length - newSectionCount;
            for (var i = 0; i < sectionsToRemoveCount; i++) {
                self.sections().pop();
            }
        }

        for (var i = 0; i < sections.length; i++) {
            ko.mapping.fromJS(newSectionWidthMeasurement, {}, sections[i].width);
        };
    };
    self.contactInfo = ko.observable("");
    self.save = function () {
        var url = "OrderItem/Save/" + self.orderId;
        return $.ajax({
            cache: false,
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: url,
            data: JSON.stringify(ko.mapping.toJS(self))
        })
            .done(function () {
                alert("Window saved.");
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown || textStatus || "Save failed.");
            });
    };
    self.complete = function () {
        var url = "OrderItem/Complete/" + self.orderId;
        return $.ajax({
            cache: false,
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            url: url,
            data: JSON.stringify(ko.mapping.toJS(self))
        })
            .done(function (result) {
                var price = result && typeof result.authoritativePrice === "number"
                    ? result.authoritativePrice.toFixed(2)
                    : "0.00";
                self.price("$" + price);

                if (result && result.sessionStatus === "Completed") {
                    try {
                        window.parent.postMessage({
                            type: "window.configurator.session.completed",
                            sessionId: result.sessionId,
                            authoritativePrice: result.authoritativePrice,
                            completedAt: result.completedAt
                        }, "*");
                    } catch (e) {
                        // not in an iframe — ignore
                    }
                }

                alert("Window submitted. Authoritative price: $" + price);
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                var response = jqXHR && jqXHR.responseJSON ? jqXHR.responseJSON : null;
                var validationErrors = response && response.validationErrors ? response.validationErrors : null;
                if (validationErrors && validationErrors.length > 0) {
                    var lines = [];
                    for (var i = 0; i < validationErrors.length; i++) {
                        var error = validationErrors[i];
                        var message = error && error.message ? error.message : "Validation failed.";
                        lines.push((i + 1) + ". " + message);
                    }
                    alert("Window could not be submitted:\n" + lines.join("\n"));
                    return;
                }

                alert(errorThrown || textStatus || "Submit failed.");
            });
    };
    self.getSpec = function () {
        var res = "";
        res += "Contact Info: " + self.contactInfo() + "\r\n";
        res += "Price: " + self.price() + "\n";
        res += "Product Line: " + self.productLine.name() + "\n";
        res += "Width: " + self.widthDescription() + "\n";
        res += "Height: " + self.heightDescription() + "\n";
        res += "Location: " + self.location() + "\n";
        res += "Frame Color: " + self.frameColor.name() + "\n";
        res += "Brickmould: " + self.brickmouldStyle.name() + "\n";
        res += "Jamd Depth: " + self.jambDepth.name() + "\n";
        res += "Pane Configuration: " + self.paneConfiguration.name() + "\n";
        res += "Frame Color: " + self.frameColor.name() + "\n";

        res += "Number of Sections: " + self.sections().length;
        for (var i = 0; i < self.sections().length; i++) {
            res += "\r\nSection " + (i + 1) + "\n";
            res += "Width: " + self.sections()[i].widthDescription() + "\n";
            res += "Operation: " + self.sections()[i].style.name() + "\n";
            res += "Crank: " + self.sections()[i].crank.name() + "\n";
            res += "Grille Pattern: " + self.sections()[i].grillePattern.name() + ", ";
            res += "Grille Color: " + self.sections()[i].grilleColor.name() + ", ";
            res += "Grille Size: " + self.sections()[i].grilleColor.name() + "\n";
            res += "SDL Pattern: " + self.sections()[i].sdlPattern.name() + ", ";
            res += "SDL Color: " + self.sections()[i].sdlColor.name() + ", ";
            res += "SDL Size: " + self.sections()[i].sdlColor.name() + "\n";
        }

        return res;
    };
    //self.mailTo = function () {
    //  return "mailto:klingenberg.d+stjohn@gmail.com?subject=quote request&body=" + self.getSpec();
    //};
    //self.onMailToClicked = function () {
    //  window.location.href = self.mailTo();
    //  $('#quote-modal').modal('toggle');
    //  ga('send', 'event', 'site', 'quote-submit');
    //  //setTimeout(function () { win.close() }, 500);
    //  // alert("We will send a quote to your email address.  Although we can order your windows at the price we send you, full payment is required before we will be able to place your order.");
    //}
};
