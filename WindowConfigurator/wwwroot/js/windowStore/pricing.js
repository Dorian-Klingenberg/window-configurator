windowStore = windowStore || {};
windowStore.order = windowStore.order || {};
windowStore.order.item = windowStore.order.item || {};
windowStore.order.item.PriceCalculator = function () {
  var self = this;

  //
  //  For any given productline
  //
  //  Step 1. Start with section by section pricing. This will provide us with either
  //   1. A price for each section no matter the properties of the frame
  //      - Calculate the perimiter of the section 
  //      - Calculate the style multiplier based on perimiter
  //      - Calculate the grille multiplier based on perimiter
  //      - Calculate the sdl multiplier based on perimiter
  //      - Calculate the pane count multiplier based on perimiter
  //      - Calculate the pane configuration multiplier based on perimiter
  //      - Multiply all together to get the sections price
  //  2. A multiplier to be combined with the frame properties to contribute to the final price  
  //
  //  In the case where a sections price does not rely on frame properties
  //  
  //  Required for 
  //  Step 2. Calculate the frames price based on perimiter measurement, jamb depth, brickmould and possibly color
  //  

  self.calculatePrice = function (windowModel, priceView) {
    self.loadPriceInfo()
      .done(function (data, textStatus, jqXHR) {
        priceInfo = data.priceInfo;
        //        self.confgureModelForOsm(windowModel);
        priceView(self.getPrice(windowModel, priceInfo));
      })
      .fail(function (jqXHR, textStatus, errorThrown) {
        self.loadPriceFailed(errorThrown);
      });
  };
  self.loadPriceInfo = function () {
    var url = "OrderItem/PriceInfo";
    return $.ajax({
      cache: false,
      type: "GET",
      dataType: "json",
      url: url
    });
  };
  self.loadPriceFailed = function (errorThrown) {
    alert(errorThrown);
  };
  self.confgureModelForOsm = function (windowModel) {
    var inchMeasurement = new windowStore.measurement.measurement();
    inchMeasurement.init(1, 1, 0, 1);

    var frameWidth = new windowStore.measurement.measurement();
    frameWidth.init(
      windowModel.width.sign,
      windowModel.width.whole,
      windowModel.width.numerator,
      windowModel.width.denominator);
    var frameHeight = new windowStore.measurement.measurement();
    frameHeight.init(
      windowModel.height.sign,
      windowModel.height.whole,
      windowModel.height.numerator,
      windowModel.height.denominator);

    frameWidth = frameWidth.subtract(inchMeasurement);
    frameHeight = frameHeight.subtract(inchMeasurement);

    windowModel.width.sign = frameWidth.sign();
    windowModel.width.whole = frameWidth.whole();
    windowModel.width.numerator = frameWidth.numerator();
    windowModel.width.denominator = frameWidth.denominator();

    windowModel.height.sign = frameHeight.sign();
    windowModel.height.whole = frameHeight.whole();
    windowModel.height.numerator = frameHeight.numerator();
    windowModel.height.denominator = frameHeight.denominator();

    for (var i = 0; i < windowModel.sections.length; i++) {
      var section = windowModel.sections[i];
      var sectionWidth = new windowStore.measurement.measurement();
      sectionWidth.init(
        section.width.sign,
        section.width.whole,
        section.width.numerator,
        section.width.denominator);

      var sectionHeight = new windowStore.measurement.measurement();
      sectionHeight.init(
        section.height.sign,
        section.height.whole,
        section.height.numerator,
        section.height.denominator);

      sectionWidth = sectionWidth.subtract(inchMeasurement);
      sectionHeight = sectionHeight.subtract(inchMeasurement);

      section.width.sign = sectionWidth.sign();
      section.width.whole = sectionWidth.whole();
      section.width.numerator = sectionWidth.numerator();
      section.width.denominator = sectionWidth.denominator();

      section.height.sign = sectionHeight.sign();
      section.height.whole = sectionHeight.whole();
      section.height.numerator = sectionHeight.numerator();
      section.height.denominator = sectionHeight.denominator();
    }
  };












  self.getPrice = function (windowModel, priceInfo) {

    var price = 0;

    // product line

    var priceInfoForProductline = self.getPriceInfoForProductLine(windowModel, priceInfo);
    if (!priceInfoForProductline) {
      return false;
    }

    // Frame
    // Keep very simple for now, if frame color isn't white apply pricePerSquareInch to lineal inches of RO

    price += self.getPriceForFrame(windowModel, priceInfoForProductline);

    // Sections
    var priceInfoForSections = self.getPriceInfoForSections(windowModel, priceInfoForProductline);
    if (!priceInfoForSections) {
      return false;
    }
    for (var i = 0; i < priceInfoForSections.sectionPrices.length; i++) {
      //var derp = self.getPriceForSection(priceInfoForSections.sectionPrices[i].priceBreakpoints);
      price += self.getPriceForSection(priceInfoForSections.sectionPrices[i].priceBreakpoints);
    }

    // Cranks
    var priceInfoForCranks = self.getPriceInfoForCranks(windowModel, priceInfoForProductline);
    if (!priceInfoForCranks) {
      return false;
    }
    for (var i = 0; i < priceInfoForCranks.crankPrices.length; i++) {
      price += priceInfoForCranks.crankPrices[i].price;
    }

    // Grille Patterns
    var priceInfoForGrilles = self.getPriceInfoForGrilles(windowModel, priceInfoForProductline);
    if (!priceInfoForGrilles) {
      return false;
    }
    for (var i = 0; i < priceInfoForGrilles.grillePrices.length; i++) {
      price += self.getPriceForSection(priceInfoForGrilles.grillePrices[i].priceBreakpoints);
    }

    // SDL Patterns
    var priceInfoForSdls = self.getPriceInfoForSdls(windowModel, priceInfoForProductline);
    if (!priceInfoForSdls) {
      return false;
    }
    for (var i = 0; i < priceInfoForSdls.sdlPrices.length; i++) {
      price += self.getPriceForSection(priceInfoForSdls.sdlPrices[i].priceBreakpoints);
    }

    // brickmould
    var priceInfoBrickmould = self.getPriceInfoForBrickmould(windowModel, priceInfoForProductline);
    if (!priceInfoBrickmould) {
      return false;
    }
    price += self.getPriceForBrickmould(priceInfoBrickmould.priceBreakpoints);

    // jamb extension
    price += self.getPriceForJamb(windowModel, priceInfoForProductline);

    // Pane Configurations
    var priceInfoForPaneConfigurations = self.getPriceInfoForPaneConfigurations(windowModel, priceInfoForProductline);
    if (!priceInfoForPaneConfigurations) {
      return false;
    }
    for (var i = 0; i < priceInfoForPaneConfigurations.paneConfigurationPrices.length; i++) {
      price += self.getPriceForSection(priceInfoForPaneConfigurations.paneConfigurationPrices[i].priceBreakpoints);
    }

    // Adjustment
    price += price * priceInfoForProductline.adjustment;

    // Markup
    price += price * priceInfoForProductline.markup;


    // return price as a 2 decimal string
    price = "" + price;
    return "$" + parseFloat(price).toFixed(2);
  };

  // Frame

  self.getPriceForFrame = function (windowModel, pricingProductLine) {

    // Calculate the lineal inches of the frame

    var frameWidthMeasurement = new windowStore.measurement.measurement().init(
      windowModel.osmWidth.sign,
      windowModel.osmWidth.whole,
      windowModel.osmWidth.numerator,
      windowModel.osmWidth.denominator
      );

    var frameHeightMeasurement = new windowStore.measurement.measurement().init(
      windowModel.osmHeight.sign,
      windowModel.osmHeight.whole,
      windowModel.osmHeight.numerator,
      windowModel.osmHeight.denominator
      );

    var frameWidthDecimal = frameWidthMeasurement.getDecimal();
    var frameHeightDecimal = frameHeightMeasurement.getDecimal();
    var frameLinealInches = (frameWidthDecimal * 2) + (frameHeightDecimal * 2);

    // get the pricing for the frame color

    var frameColorPricePerInch = 0;
    for (var i = 0; i < pricingProductLine.frameColors.length; i++) {
      if (windowModel.frameColor.name === pricingProductLine.frameColors[i].name) {
        frameColorPricePerInch = pricingProductLine.frameColors[i].pricePerInch;
      }
    }

    return ((frameWidthDecimal * 2) + (frameHeightDecimal * 2)) * frameColorPricePerInch;
  }

  // Section

  self.getPriceInfoForSections = function (windowModel, pricingProductLine) {

    // loop through the sections and add the relevent breakpoint to the object
    // start by getting the correct style

    var pricingStyle;
    var section;
    var result = {
      sectionPrices: []
    };
    for (var i = 0; i < windowModel.sections.length; i++) {
      section = windowModel.sections[i];
      for (var j = 0; j < pricingProductLine.styles.length; j++) {
        if (section.style.name === pricingProductLine.styles[j].name) {
          pricingStyle = pricingProductLine.styles[j];

          // now find the breakpoint

          result.sectionPrices.push({
            section: section,
            priceBreakpoints: self.getPriceInfoBreakpoints(section.width, section.height, pricingStyle)
          });
        }
      }
    }
    return result;
  };
  self.getPriceForSection = function (sectionPricingStrategy) {
    var widthDistanceFromLower =
      sectionPricingStrategy.width -
      sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.width;
    var heightDistanceFromLower =
      sectionPricingStrategy.height -
      sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.height;

    // Determine the difference in ppi for widths at upper and lower heights

    var distanceBetweenWidthBreakpoints =
      sectionPricingStrategy.upperWidthUpperHeightBreakpoint.width -
      sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.width;

    var upperHeightWidthPpiDifference = null;
    var lowerHeightWidthPpiDifference = null;

    // If the width falls exactly on a breakpoint
    if (distanceBetweenWidthBreakpoints <= 0) {
      upperHeightWidthPpiDifference = sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch;
      lowerHeightWidthPpiDifference = sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.pricePerInch;
    } else {
      upperHeightWidthPpiDifference =
        (sectionPricingStrategy.upperWidthUpperHeightBreakpoint.pricePerInch -
          sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch) /
        distanceBetweenWidthBreakpoints;
      lowerHeightWidthPpiDifference =
        (sectionPricingStrategy.upperWidthLowerHeightBreakpoint.pricePerInch -
          sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.pricePerInch) /
        distanceBetweenWidthBreakpoints;
    }

    // Determine price for given width at upper and lower heights

    var upperHeightPpi =
      sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch +
      (widthDistanceFromLower * upperHeightWidthPpiDifference);
    var lowerHeightPpi =
      sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.pricePerInch +
      (widthDistanceFromLower * lowerHeightWidthPpiDifference);

    // Determine the difference in ppi for heights at given width

    var distanceBetweenHeightBreakpoints =
      sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.height -
      sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.height;

    var heightPpiDifference;
    if (distanceBetweenHeightBreakpoints <= 0) {
      heightPpiDifference = sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch;
    } else {
      heightPpiDifference =
            (upperHeightPpi - lowerHeightPpi) / distanceBetweenHeightBreakpoints;
    }

    var pricePerInch = lowerHeightPpi + (heightDistanceFromLower * heightPpiDifference);

    return ((sectionPricingStrategy.width * 2) + (sectionPricingStrategy.height * 2)) * pricePerInch;
  }

  // Crank

  self.getPriceInfoForCranks = function (windowModel, pricingProductLine) {
    // loop through the sections and add the relevent breakpoint to the object
    // start by getting the correct crank

    var section;
    var result = { crankPrices: [] };
    for (var i = 0; i < windowModel.sections.length; i++) {
      section = windowModel.sections[i];
      for (var j = 0; j < pricingProductLine.cranks.length; j++) {
        if (section.crank.name === pricingProductLine.cranks[j].name) {
          result.crankPrices.push({ price: pricingProductLine.cranks[j].price });
        };
      }
    }
    return result;
  };

  // Grille

  self.getPriceInfoForGrilles = function (windowModel, pricingProductLine) {
    // loop through the sections and add the relevent breakpoint to the object
    // start by getting the correct grille
    var pricingGrille;
    var section;
    var result = {
      grillePrices: []
    };

    // Get the section
    for (var i = 0; i < windowModel.sections.length; i++) {
      section = windowModel.sections[i];

      // Get the grille pattern 
      for (var j = 0; j < pricingProductLine.grillePatterns.length; j++) {
        if (section.grillePattern.name === pricingProductLine.grillePatterns[j].name) {

          pricingGrille = pricingProductLine.grillePatterns[j];

          // now find the breakpoint
          result.grillePrices.push({
            section: section,
            priceBreakpoints: self.getPriceInfoBreakpoints(section.width, section.height, pricingGrille)
          });
        }
      }
    }
    return result;
  };

  // SDL

  self.getPriceInfoForSdls = function (windowModel, pricingProductLine) {
    // loop through the sections and add the relevent breakpoint to the object
    // start by getting the correct sdl
    var pricingSdl;
    var section;
    var result = {
      sdlPrices: []
    };

    // Get the section
    for (var i = 0; i < windowModel.sections.length; i++) {
      section = windowModel.sections[i];

      // Get the sdl pattern 
      for (var j = 0; j < pricingProductLine.sdlPatterns.length; j++) {
        if (section.sdlPattern.name === pricingProductLine.sdlPatterns[j].name) {

          pricingSdl = pricingProductLine.sdlPatterns[j];

          // now find the breakpoint
          result.sdlPrices.push({
            section: section,
            priceBreakpoints: self.getPriceInfoBreakpoints(section.width, section.height, pricingSdl)
          });
        }
      }
    }
    return result;
  };

  // Brickmould

  self.getPriceInfoForBrickmould = function (windowModel, pricingProductLine) {
    // loop through the brickmould styles and add the relevent breakpoint to the object
    // start by getting the correct style
    var result = null;

    for (var i = 0; i < pricingProductLine.brickmouldStyles.length; i++) {
      if (windowModel.brickmouldStyle.name === pricingProductLine.brickmouldStyles[i].name) {
        result = {
          priceBreakpoints: self.getPriceInfoBreakpoints(
            windowModel.width,
            windowModel.height,
            pricingProductLine.brickmouldStyles[i]
          )
        };
      }
    }
    return result;
  };
  self.getPriceForBrickmould = function (sectionPricingStrategy) {
    var widthDistanceFromLower =
      sectionPricingStrategy.width -
      sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.width;
    var heightDistanceFromLower =
      sectionPricingStrategy.height -
      sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.height;

    // Determine the difference in ppi for widths at upper and lower heights

    var distanceBetweenWidthBreakpoints =
      sectionPricingStrategy.upperWidthUpperHeightBreakpoint.width -
      sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.width;

    var upperHeightWidthPpiDifference = null;
    var lowerHeightWidthPpiDifference = null;

    // If the width falls exactly on a breakpoint
    if (distanceBetweenWidthBreakpoints <= 0) {
      upperHeightWidthPpiDifference = sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch;
      lowerHeightWidthPpiDifference = sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.pricePerInch;
    } else {
      upperHeightWidthPpiDifference =
        (sectionPricingStrategy.upperWidthUpperHeightBreakpoint.pricePerInch -
          sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch) /
        distanceBetweenWidthBreakpoints;
      lowerHeightWidthPpiDifference =
        (sectionPricingStrategy.upperWidthLowerHeightBreakpoint.pricePerInch -
          sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.pricePerInch) /
        distanceBetweenWidthBreakpoints;
    }

    // Determine price for given width at upper and lower heights

    var upperHeightPpi =
      sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch +
      (widthDistanceFromLower * upperHeightWidthPpiDifference);
    var lowerHeightPpi =
      sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.pricePerInch +
      (widthDistanceFromLower * lowerHeightWidthPpiDifference);

    // Determine the difference in ppi for heights at given width

    var distanceBetweenHeightBreakpoints =
      sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.height -
      sectionPricingStrategy.lowerWidthLowerHeightBreakpoint.height;

    var heightPpiDifference;
    if (distanceBetweenHeightBreakpoints <= 0) {
      heightPpiDifference = sectionPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch;
    } else {
      heightPpiDifference =
            (upperHeightPpi - lowerHeightPpi) / distanceBetweenHeightBreakpoints;
    }

    var pricePerInch = lowerHeightPpi + (heightDistanceFromLower * heightPpiDifference);

    return ((sectionPricingStrategy.width * 2) + (sectionPricingStrategy.height * 2)) * upperHeightPpi;
  };

  self.getPriceForJamb = function (windowModel, pricingProductLine) {
    return 0;
  }


  //// Jamb Depth
  //self.getPriceInfoForJamb = function (windowModel, pricingProductLine) {
  //  // loop through the jamb styles and add the relevent breakpoint to the object
  //  // start by getting the correct style
  //  var result = null;

  //  for (var i = 0; i < pricingProductLine.jambDepths.length; i++) {
  //    if (windowModel.jambDepth.name === pricingProductLine.jambDepths[i].name) {
  //      result = {
  //        priceBreakpoints: self.getPriceInfoBreakpoints(
  //          windowModel.width,
  //          windowModel.height,
  //          pricingProductLine.jambDepths[i]
  //        )
  //      };
  //    }
  //  }
  //  return result;
  //};
  //self.getPriceForJamb = function (jambPricingStrategy) {
  //  var widthDistanceFromLower =
  //   jambPricingStrategy.width -
  //   jambPricingStrategy.lowerWidthLowerHeightBreakpoint.width;
  //  var heightDistanceFromLower =
  //   jambPricingStrategy.height -
  //   jambPricingStrategy.lowerWidthLowerHeightBreakpoint.height;

  //  // Determine the difference in ppi for widths at upper and lower heights

  //  var distanceBetweenWidthBreakpoints =
  //   jambPricingStrategy.upperWidthUpperHeightBreakpoint.width -
  //   jambPricingStrategy.lowerWidthUpperHeightBreakpoint.width;

  //  var upperHeightWidthPpiDifference = null;
  //  var lowerHeightWidthPpiDifference = null;

  //  // If the width falls exactly on a breakpoint
  //  if (distanceBetweenWidthBreakpoints <= 0) {
  //    upperHeightWidthPpiDifference = jambPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch;
  //    lowerHeightWidthPpiDifference = jambPricingStrategy.lowerWidthLowerHeightBreakpoint.pricePerInch;
  //  } else {
  //    var upperHeightWidthPpiDifference =
  //      (jambPricingStrategy.upperWidthUpperHeightBreakpoint.pricePerInch -
  //       jambPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch) /
  //      distanceBetweenWidthBreakpoints;
  //    var lowerHeightWidthPpiDifference =
  //      (jambPricingStrategy.upperWidthLowerHeightBreakpoint.pricePerInch -
  //       jambPricingStrategy.lowerWidthLowerHeightBreakpoint.pricePerInch) /
  //      distanceBetweenWidthBreakpoints;
  //  }

  //  // Determine price for given width at upper and lower heights

  //  var upperHeightPpi =
  //   jambPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch +
  //    (widthDistanceFromLower * upperHeightWidthPpiDifference);
  //  var lowerHeightPpi =
  //   jambPricingStrategy.lowerWidthLowerHeightBreakpoint.pricePerInch +
  //    (widthDistanceFromLower * lowerHeightWidthPpiDifference);

  //  // Determine the difference in ppi for heights at given width

  //  var distanceBetweenHeightBreakpoints =
  //   jambPricingStrategy.lowerWidthUpperHeightBreakpoint.height -
  //   jambPricingStrategy.lowerWidthLowerHeightBreakpoint.height;

  //  var heightPpiDifference;
  //  if (distanceBetweenHeightBreakpoints <= 0) {
  //    heightPpiDifference = jambPricingStrategy.lowerWidthUpperHeightBreakpoint.pricePerInch;
  //  } else {
  //    heightPpiDifference =
  //      (upperHeightPpi - lowerHeightPpi) / distanceBetweenHeightBreakpoints;
  //  }

  //  var pricePerInch = lowerHeightPpi + (heightDistanceFromLower * heightPpiDifference);

  //  return ((jambPricingStrategy.width * 2) + (jambPricingStrategy.height * 2)) * pricePerInch;
  //};

  // Pane Configuration

  self.getPriceInfoForPaneConfigurations = function (windowModel, pricingProductLine) {
    // loop through the sections and add the relevent breakpoint to the object
    // start by getting the correct pane configuration
    var pricingPaneConfiguration;
    var section;
    var result = {
      paneConfigurationPrices: []
    };

    // Get the section
    for (var i = 0; i < windowModel.sections.length; i++) {
      section = windowModel.sections[i];

      // Get the configuration 
      for (var j = 0; j < pricingProductLine.paneConfigurations.length; j++) {
        if (windowModel.paneConfiguration.name === pricingProductLine.paneConfigurations[j].name) {
          pricingPaneConfiguration = pricingProductLine.paneConfigurations[j];

          // now find the breakpoint
          result.paneConfigurationPrices.push({
            section: section,
            priceBreakpoints: self.getPriceInfoBreakpoints(section.width, section.height, pricingPaneConfiguration)
          });
        }
      }
    }
    return result;
  };

  // Util

  self.getPriceInfoForProductLine = function (windowModel, priceInfo) {
    var manufacturerName = windowModel.productLine.manufacturerName;
    var productLineName = windowModel.productLine.name;
    var pricingProductline;

    for (var i = 0; i < priceInfo.manufacturers.length; i++) {
      if (priceInfo.manufacturers[i].name === manufacturerName) {
        for (var j = 0; j < priceInfo.manufacturers[i].productLines.length; j++) {
          if (priceInfo.manufacturers[i].productLines[j].name === productLineName) {
            pricingProductline = priceInfo.manufacturers[i].productLines[j];
          }
        }
      }
    }
    return pricingProductline;
  };
  self.getPriceInfoBreakpoints = function (width, height, pricingStyle) {
    var widthMeasurement = new windowStore.measurement.measurement();
    widthMeasurement.init(
      width.sign,
      width.whole,
      width.numerator,
      width.denominator);
    var heightMeasurement = new windowStore.measurement.measurement();
    heightMeasurement.init(
      height.sign,
      height.whole,
      height.numerator,
      height.denominator);
    var widthMeasurementDecimal = widthMeasurement.getDecimal();
    var heightMeasurementDecimal = heightMeasurement.getDecimal();

    var lowerWidthBreakpoint = pricingStyle.widthBreakpoints[0];
    var upperWidthBreakpoint;
    for (var i = 0; i < pricingStyle.widthBreakpoints.length; ++i) {
      upperWidthBreakpoint = pricingStyle.widthBreakpoints[i];
      if (widthMeasurementDecimal >= upperWidthBreakpoint.width) {
        lowerWidthBreakpoint = upperWidthBreakpoint;
      } else {
        break;
      }
    }

    var lowerWidthLowerHeightBreakpoint = pricingStyle.widthBreakpoints[0];
    var lowerWidthUpperHeightBreakpoint;
    for (var i = 0; i < lowerWidthBreakpoint.heightBreakpoints.length; ++i) {
      lowerWidthUpperHeightBreakpoint = lowerWidthBreakpoint.heightBreakpoints[i];
      if (heightMeasurementDecimal >= lowerWidthUpperHeightBreakpoint.height) {
        lowerWidthLowerHeightBreakpoint = lowerWidthUpperHeightBreakpoint;
      } else {
        break;
      }
    }

    var upperWidthLowerHeightBreakpoint = pricingStyle.widthBreakpoints[0];
    var upperWidthUpperHeightBreakpoint;
    for (var i = 0; i < upperWidthBreakpoint.heightBreakpoints.length; ++i) {
      upperWidthUpperHeightBreakpoint = upperWidthBreakpoint.heightBreakpoints[i];
      if (heightMeasurementDecimal >= upperWidthUpperHeightBreakpoint.height) {
        upperWidthLowerHeightBreakpoint = upperWidthUpperHeightBreakpoint;
      } else {
        break;
      }
    }

    return {
      width: widthMeasurement.getDecimal(),
      height: heightMeasurement.getDecimal(),
      lowerWidthLowerHeightBreakpoint: {
        width: lowerWidthBreakpoint.width,
        height: lowerWidthLowerHeightBreakpoint.height,
        pricePerInch: lowerWidthLowerHeightBreakpoint.pricePerInch
      },
      lowerWidthUpperHeightBreakpoint: {
        width: lowerWidthBreakpoint.width,
        height: lowerWidthUpperHeightBreakpoint.height,
        pricePerInch: lowerWidthUpperHeightBreakpoint.pricePerInch
      },
      upperWidthLowerHeightBreakpoint: {
        width: upperWidthBreakpoint.width,
        height: upperWidthLowerHeightBreakpoint.height,
        pricePerInch: upperWidthLowerHeightBreakpoint.pricePerInch
      },
      upperWidthUpperHeightBreakpoint: {
        width: upperWidthBreakpoint.width,
        height: upperWidthUpperHeightBreakpoint.height,
        pricePerInch: upperWidthUpperHeightBreakpoint.pricePerInch
      }
    };
  };
};