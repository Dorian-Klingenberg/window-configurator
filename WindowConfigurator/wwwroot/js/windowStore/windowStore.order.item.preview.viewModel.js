var windowStore = windowStore || {};
windowStore.order = windowStore.order || {};
windowStore.order.item = windowStore.order.item || {};
windowStore.order.item.preview = windowStore.order.item.preview || {};
windowStore.order.item.preview.viewModel = windowStore.order.item.preview.viewModel || {};

windowStore.order.item.preview.viewModel.Mullion = function () {
  var self = this;

  self.imageWidth = ko.observable(0);
  self.x = ko.observable(0);
  self.y = ko.observable(0);
  self.width = ko.observable(0);
  self.height = ko.observable(0);

  self.image = ko.observable("");
  self.imagePatternId = ko.observable("");
  self.imagePatternUrl = ko.computed(function () {
    return "url(#" + self.imagePatternId() + ")";
  });
  self.color = ko.observable("");
  self.opacity = ko.observable("");
};

windowStore.order.item.preview.viewModel.Brickmould = function () {
  var self = this;

  self.imageWidth = ko.observable(0);
  self.x = ko.observable(0);
  self.y = ko.observable(0);
  self.width = ko.observable(0);
  self.height = ko.observable(0);
  self.enclosureWidth = ko.observable(0);

  // Top Left

  {
    self.topLeftImagePatternId = ko.observable("");
    self.topLeftImagePatternUrl = ko.computed(function () {
      return "url(#" + self.topLeftImagePatternId() + ")";
    });
    self.topLeftImage = ko.observable("");
    self.topLeftX = ko.computed(function () {
      return parseInt(self.x());
    });
    self.topLeftY = ko.computed(function () {
      return parseInt(self.y());
    });
  }

  // Top Right

  {
    self.topRightImagePatternId = ko.observable("");
    self.topRightImagePatternUrl = ko.computed(function () {
      return "url(#" + self.topRightImagePatternId() + ")";
    });
    self.topRightImage = ko.observable("");
    self.topRightX = ko.computed(function () {
      return parseInt(self.x()) + parseInt(self.imageWidth()) + parseInt(self.width());
    });
    self.topRightY = ko.computed(function () {
      return parseInt(self.y());
    });
  }

  // Bottom Right

  {
    self.bottomRightImagePatternId = ko.observable("");
    self.bottomRightImagePatternUrl = ko.computed(function () {
      return "url(#" + self.bottomRightImagePatternId() + ")";
    });
    self.bottomRightImage = ko.observable("");
    self.bottomRightX = ko.computed(function () {
      return parseInt(self.x()) + parseInt(self.imageWidth()) + parseInt(self.width());
    });
    self.bottomRightY = ko.computed(function () {
      return parseInt(self.y()) + parseInt(self.imageWidth()) + parseInt(self.height());
    });
  }

  // Bottom Left

  {
    self.bottomLeftImagePatternId = ko.observable("");
    self.bottomLeftImagePatternUrl = ko.computed(function () {
      return "url(#" + self.bottomLeftImagePatternId() + ")";
    });
    self.bottomLeftImage = ko.observable("");
    self.bottomLeftX = ko.computed(function () {
      return parseInt(self.x());
    });
    self.bottomLeftY = ko.computed(function () {
      return parseInt(self.y()) + parseInt(self.imageWidth()) + parseInt(self.height());
    });
  }

  // Top

  {
    self.topImagePatternId = ko.observable("");
    self.topImagePatternUrl = ko.computed(function () {
      return "url(#" + self.topImagePatternId() + ")";
    });
    self.topImage = ko.observable("");
    self.topX = ko.computed(function () {
      return parseInt(self.x()) + parseInt(self.imageWidth());
    });
    self.topY = ko.computed(function () {
      return parseInt(self.y());
    });
  }

  // Right

  {
    self.rightImagePatternId = ko.observable("");
    self.rightImagePatternUrl = ko.computed(function () {
      return "url(#" + self.rightImagePatternId() + ")";
    });
    self.rightImage = ko.observable("");
    self.rightX = ko.computed(function () {
      return parseInt(self.x()) + parseInt(self.imageWidth()) + parseInt(self.width());
    });
    self.rightY = ko.computed(function () {
      return parseInt(self.y()) + parseInt(self.imageWidth());
    });
  }

  // Bottom

  {
    self.bottomImagePatternId = ko.observable("");
    self.bottomImagePatternUrl = ko.computed(function () {
      return "url(#" + self.bottomImagePatternId() + ")";
    });
    self.bottomImage = ko.observable("");
    self.bottomX = ko.computed(function () {
      return parseInt(self.x()) + parseInt(self.imageWidth());
    })
    self.bottomY = ko.computed(function () {
      return parseInt(self.y()) + parseInt(self.imageWidth()) + parseInt(self.height());
    });
  }

  // Left

  {
    self.leftImagePatternId = ko.observable("");
    self.leftImagePatternUrl = ko.computed(function () {
      return "url(#" + self.leftImagePatternId() + ")";
    });
    self.leftImage = ko.observable("");
    self.leftX = ko.computed(function () {
      return parseInt(self.x());
    });
    self.leftY = ko.computed(function () {
      return parseInt(self.y()) + parseInt(self.imageWidth());
    });
  }

  self.color = ko.observable("");
  self.opacity = ko.observable("");
};

windowStore.order.item.preview.viewModel.Section = function () {
  var self = this;

  self.imageWidth = ko.observable(0);
  self.x = ko.observable(0);
  self.y = ko.observable(0);
  self.width = ko.observable(0);
  self.height = ko.observable(0);
  self.enclosureWidth = ko.observable(0);
  self.overlayColor = ko.observable("lightsteelblue");
  self.overlayOpacity = ko.observable("0.0");

  self.grilleColor = ko.observable("");
  self.grilleSize = ko.observable("");
  self.grillePattern = null;

  self.sdlColor = ko.observable("");
  self.sdlSize = ko.observable("");
  self.sdlPattern = null;

  self.styleLines = null;
  self.hasVerticalSlider = ko.observable(false);
  self.hasHorizontalSlider = ko.observable(false);
  self.sliderImageWidth = ko.observable(0);
  self.sliderImage = ko.observable("");

  // Top Left

  {
    self.topLeftImagePatternId = ko.observable("");
    self.topLeftImagePatternUrl = ko.computed(function () {
      return "url(#" + self.topLeftImagePatternId() + ")";
    });
    self.topLeftImage = ko.observable("");
    self.topLeftX = ko.computed(function () {
      return self.x();
    });
    self.topLeftY = ko.computed(function () {
      return self.y();
    });
    self.topLeftWidth = ko.computed(function () {
      return self.imageWidth();
    });
    self.topLeftHeight = ko.computed(function () {
      return self.imageWidth();
    });
  }

  // Top Right

  {
    self.topRightImagePatternId = ko.observable("");
    self.topRightImagePatternUrl = ko.computed(function () {
      return "url(#" + self.topRightImagePatternId() + ")";
    });
    self.topRightImage = ko.observable("");
    self.topRightX = ko.computed(function () {
      return self.x() + self.width() - self.imageWidth();
    });
    self.topRightY = ko.computed(function () {
      return self.y();
    });
    self.topRightWidth = ko.computed(function () {
      return self.imageWidth();
    });
    self.topRightHeight = ko.computed(function () {
      return self.imageWidth();
    });
  }

  // Bottom Right

  {
    self.bottomRightImagePatternId = ko.observable("");
    self.bottomRightImagePatternUrl = ko.computed(function () {
      return "url(#" + self.bottomRightImagePatternId() + ")";
    });
    self.bottomRightImage = ko.observable("");
    self.bottomRightX = ko.computed(function () {
      return self.x() + self.width() - self.imageWidth();
    });
    self.bottomRightY = ko.computed(function () {
      return self.y() + self.height() - self.imageWidth();
    });
    self.bottomRightWidth = ko.computed(function () {
      return self.imageWidth();
    });
    self.bottomRightHeight = ko.computed(function () {
      return self.imageWidth();
    });
  }

  // Bottom Left

  {
    self.bottomLeftImagePatternId = ko.observable("");
    self.bottomLeftImagePatternUrl = ko.computed(function () {
      return "url(#" + self.bottomLeftImagePatternId() + ")";
    });
    self.bottomLeftImage = ko.observable("");
    self.bottomLeftX = ko.computed(function () {
      return self.x();
    });
    self.bottomLeftY = ko.computed(function () {
      return self.y() + self.height() - self.imageWidth();
    });
    self.bottomLeftWidth = ko.computed(function () {
      return self.imageWidth();
    });
    self.bottomLeftHeight = ko.computed(function () {
      return self.imageWidth();
    });
  }

  // Top

  {
    self.topImagePatternId = ko.observable("");
    self.topImagePatternUrl = ko.computed(function () {
      return "url(#" + self.topImagePatternId() + ")";
    });
    self.topImage = ko.observable("");
    self.topX = ko.computed(function () {
      return self.x() + self.imageWidth();
    });
    self.topY = ko.computed(function () {
      return self.y();
    });
    self.topWidth = ko.computed(function () {
      return self.width() - (2 * self.imageWidth());
    });
    self.topHeight = ko.computed(function () {
      return self.imageWidth();
    })
  }

  // Right

  {
    self.rightImagePatternId = ko.observable("");
    self.rightImagePatternUrl = ko.computed(function () {
      return "url(#" + self.rightImagePatternId() + ")";
    });
    self.rightImage = ko.observable("");
    self.rightX = ko.computed(function () {
      return self.x() + self.width() - self.imageWidth();
    });
    self.rightY = ko.computed(function () {
      return self.y() + self.imageWidth();
    });
    self.rightWidth = ko.computed(function () {
      return self.imageWidth();
    });
    self.rightHeight = ko.computed(function () {
      return self.height() - (2 * self.imageWidth());
    });
  }

  // Bottom

  {
    self.bottomImagePatternId = ko.observable("");
    self.bottomImagePatternUrl = ko.computed(function () {
      return "url(#" + self.bottomImagePatternId() + ")";
    });
    self.bottomImage = ko.observable("");
    self.bottomX = ko.computed(function () {
      return self.x() + self.imageWidth();
    })
    self.bottomY = ko.computed(function () {
      return self.y() + self.height() - self.imageWidth();
    });
    self.bottomWidth = ko.computed(function () {
      return self.width() - (2 * self.imageWidth());
    });
    self.bottomHeight = ko.computed(function () {
      return self.imageWidth();
    });
  }

  // Left

  {
    self.leftImagePatternId = ko.observable("");
    self.leftImagePatternUrl = ko.computed(function () {
      return "url(#" + self.leftImagePatternId() + ")";
    });
    self.leftImage = ko.observable("");
    self.leftX = ko.computed(function () {
      return self.x();
    });
    self.leftY = ko.computed(function () {
      return self.y() + self.imageWidth();
    });
    self.leftWidth = ko.computed(function () {
      return self.imageWidth();
    });
    self.leftHeight = ko.computed(function () {
      return self.height() - (2 * self.imageWidth());
    });
  }

  // Horizontal Slider

  {
    self.horizontalSliderImagePatternId = ko.observable("");
    self.horizontalSliderImagePatternUrl = ko.computed(function () {
      return "url(#" + self.horizontalSliderImagePatternId() + ")";
    });
    self.sliderImage = ko.observable("");
    self.horizontalSliderX = ko.computed(function () {
      return self.x();
    });
    self.horizontalSliderY = ko.computed(function () {
      return self.y() + (self.height() / 2) - (self.sliderImageWidth() / 2);
    });
    self.horizontalSliderWidth = ko.computed(function () {
      return self.width();
      //return self.width();
    });
    self.horizontalSliderHeight = ko.computed(function () {
      return self.sliderImageWidth();
    });
  }

  // Vertical Slider

  {
    self.verticalSliderImagePatternId = ko.observable("");
    self.verticalSliderImagePatternUrl = ko.computed(function () {
      return "url(#" + self.verticalSliderImagePatternId() + ")";
    });
    self.verticalSliderImage = ko.observable("");
    self.verticalSliderX = ko.computed(function () {
      return self.x() + (self.width() / 2) - (self.sliderImageWidth() / 2);
    });
    self.verticalSliderY = ko.computed(function () {
      return self.y();
    });
    self.verticalSliderWidth = ko.computed(function () {
      return self.sliderImageWidth();
    });
    self.verticalSliderHeight = ko.computed(function () {
      return self.height();
    });
  }

  self.color = ko.observable("");
  self.opacity = ko.observable("");
};

windowStore.order.item.preview.viewModel.Frame = function () {
  var self = this;

  self.imageWidth = ko.observable(0);
  self.x = ko.observable(0);
  self.y = ko.observable(0);
  self.width = ko.observable(0);
  self.height = ko.observable(0);
  self.enclosureWidth = ko.observable(0);

  // Top Left

  {
    self.topLeftImagePatternId = ko.observable("");
    self.topLeftImagePatternUrl = ko.computed(function () {
      return "url(#" + self.topLeftImagePatternId() + ")";
    });
    self.topLeftImage = ko.observable("");
    self.topLeftX = ko.computed(function () {
      return parseInt(self.x());
    });
    self.topLeftY = ko.computed(function () {
      return parseInt(self.y());
    });
  }

  // Top Right

  {
    self.topRightImagePatternId = ko.observable("");
    self.topRightImagePatternUrl = ko.computed(function () {
      return "url(#" + self.topRightImagePatternId() + ")";
    });
    self.topRightImage = ko.observable("");
    self.topRightX = ko.computed(function () {
      return parseInt(self.x()) + parseInt(self.imageWidth()) + parseInt(self.width());
    });
    self.topRightY = ko.computed(function () {
      return parseInt(self.y());
    });
  }

  // Bottom Right

  {
    self.bottomRightImagePatternId = ko.observable("");
    self.bottomRightImagePatternUrl = ko.computed(function () {
      return "url(#" + self.bottomRightImagePatternId() + ")";
    });
    self.bottomRightImage = ko.observable("");
    self.bottomRightX = ko.computed(function () {
      return parseInt(self.x()) + parseInt(self.imageWidth()) + parseInt(self.width());
    });
    self.bottomRightY = ko.computed(function () {
      return parseInt(self.y()) + parseInt(self.imageWidth()) + parseInt(self.height());
    });
  }

  // Bottom Left

  {
    self.bottomLeftImagePatternId = ko.observable("");
    self.bottomLeftImagePatternUrl = ko.computed(function () {
      return "url(#" + self.bottomLeftImagePatternId() + ")";
    });
    self.bottomLeftImage = ko.observable("");
    self.bottomLeftX = ko.computed(function () {
      return parseInt(self.x());
    });
    self.bottomLeftY = ko.computed(function () {
      return parseInt(self.y()) + parseInt(self.imageWidth()) + parseInt(self.height());
    });
  }

  // Top

  {
    self.topImagePatternId = ko.observable("");
    self.topImagePatternUrl = ko.computed(function () {
      return "url(#" + self.topImagePatternId() + ")";
    });
    self.topImage = ko.observable("");
    self.topX = ko.computed(function () {
      return parseInt(self.x()) + parseInt(self.imageWidth());
    });
    self.topY = ko.computed(function () {
      return parseInt(self.y());
    });
  }

  // Right

  {
    self.rightImagePatternId = ko.observable("");
    self.rightImagePatternUrl = ko.computed(function () {
      return "url(#" + self.rightImagePatternId() + ")";
    });
    self.rightImage = ko.observable("");
    self.rightX = ko.computed(function () {
      return parseInt(self.x()) + parseInt(self.imageWidth()) + parseInt(self.width());
    });
    self.rightY = ko.computed(function () {
      return parseInt(self.y()) + parseInt(self.imageWidth());
    });
  }

  // Bottom

  {
    self.bottomImagePatternId = ko.observable("");
    self.bottomImagePatternUrl = ko.computed(function () {
      return "url(#" + self.bottomImagePatternId() + ")";
    });
    self.bottomImage = ko.observable("");
    self.bottomX = ko.computed(function () {
      return parseInt(self.x()) + parseInt(self.imageWidth());
    })
    self.bottomY = ko.computed(function () {
      return parseInt(self.y()) + parseInt(self.imageWidth()) + parseInt(self.height());
    });
  }

  // Left

  {
    self.leftImagePatternId = ko.observable("");
    self.leftImagePatternUrl = ko.computed(function () {
      return "url(#" + self.leftImagePatternId() + ")";
    });
    self.leftImage = ko.observable("");
    self.leftX = ko.computed(function () {
      return parseInt(self.x());
    });
    self.leftY = ko.computed(function () {
      return parseInt(self.y()) + parseInt(self.imageWidth());
    });
  }

  self.color = ko.observable("");
  self.opacity = ko.observable("");

  self.sections = ko.observableArray([]);
  self.mullions = ko.observableArray([]);
};

windowStore.order.item.preview.viewModel.WindowPreview = function () {
  var self = this;

  self.sectionSelectedCallback = null;

  self.scale = ko.observable(1);
  self.x = ko.observable(1);
  self.y = ko.observable(1);

  self.osmHeight = ko.observable(1);
  self.osmWidth = ko.observable(1);

  self.frame = new windowStore.order.item.preview.viewModel.Frame();
  self.brickmould = new windowStore.order.item.preview.viewModel.Brickmould();
  
  self.update = function (window) {

    var frame = self.frame;

    // Set up the width of the frames enclosure

    {
      var enclosureWidthMeasurement = new windowStore.measurement.measurement().init(
        window.productLine.enclosureWidth.sign,
        window.productLine.enclosureWidth.whole,
        window.productLine.enclosureWidth.numerator,
        window.productLine.enclosureWidth.denominator
      );
      var enclosureWidthDecimal = enclosureWidthMeasurement.getDecimal();
      frame.enclosureWidth(enclosureWidthDecimal * self.scale());
    }

    // Convert osmWidth and osmHeight to pixels and set the observable values

    {
      var osmWidthMeasurement = new windowStore.measurement.measurement().init(
        window.osmWidth.sign,
        window.osmWidth.whole,
        window.osmWidth.numerator,
        window.osmWidth.denominator
      );

      var osmHeightMeasurement = new windowStore.measurement.measurement().init(
        window.osmHeight.sign,
        window.osmHeight.whole,
        window.osmHeight.numerator,
        window.osmHeight.denominator
      );

      var osmWidthDecimal = osmWidthMeasurement.getDecimal();
      var osmHeightDecimal = osmHeightMeasurement.getDecimal();

      self.osmWidth(osmWidthDecimal * self.scale());
      self.osmHeight(osmHeightDecimal * self.scale());

      frame.x(self.x());
      frame.y(self.y());
      frame.width(self.osmWidth() - (2 * frame.enclosureWidth()));
      frame.height(self.osmHeight() - (2 * frame.enclosureWidth()));
    }

    // Set up the frames enclosure image textures

    {
      frame.imageWidth(frame.enclosureWidth());
      frame.topLeftImage(window.productLine.imageTopLeft);
      frame.topRightImage(window.productLine.imageTopRight);
      frame.bottomRightImage(window.productLine.imageBottomRight);
      frame.bottomLeftImage(window.productLine.imageBottomLeft);
      frame.topImage(window.productLine.imageTop);
      frame.rightImage(window.productLine.imageRight);
      frame.bottomImage(window.productLine.imageBottom);
      frame.leftImage(window.productLine.imageLeft);

      frame.topLeftImagePatternId("top-left-frame");
      frame.topRightImagePatternId("top-right-frame");
      frame.bottomRightImagePatternId("bottom-right-frame");
      frame.bottomLeftImagePatternId("bottom-left-frame");
      frame.leftImagePatternId("left-frame");
      frame.topImagePatternId("top-frame");
      frame.rightImagePatternId("right-frame");
      frame.bottomImagePatternId("bottom-frame");

      frame.color(window.frameColor.fillColor);
      frame.opacity(window.frameColor.fillOpacity);
    }

    // Set Up the Width of the Mullion

    var mullionWidthMeasurement = new windowStore.measurement.measurement().init(
      window.productLine.mullionWidth.sign,
      window.productLine.mullionWidth.whole,
      window.productLine.mullionWidth.numerator,
      window.productLine.mullionWidth.denominator
    );
    var mullionWidthDecimal = mullionWidthMeasurement.getDecimal();

    // Set up sections and add them to list

    {
      frame.sections.splice(0, frame.sections().length);
      var currentSectionX = frame.x() + frame.enclosureWidth();
      var currentSectionY = frame.y() + frame.enclosureWidth();

      var totalMullionWidth = ((window.sections.length - 1) * (mullionWidthDecimal * self.scale()));
      var widthTrim = (totalMullionWidth / window.sections.length) + ((frame.enclosureWidth() * 2) / window.sections.length);
      var heightTrim = frame.enclosureWidth() * 2;

      for (var i = 0; i < window.sections.length; i++) {
        var currPrevSection = new windowStore.order.item.preview.viewModel.Section();

        currPrevSection.col = window.sections[i].col;

        // Set up section position and dimensions

        self.updateSectionDimensions(window.sections[i], currPrevSection, currentSectionX, currentSectionY, widthTrim, heightTrim);

        // Position the next section to the right of this one and add the mullion width

        currentSectionX += currPrevSection.width() + (mullionWidthDecimal * self.scale());

        // Set up the width of the sections enclosure

        self.updateSectionEnclosure(window.sections[i], currPrevSection);

        // Set up section enclosure image textures

        self.updateSectionEnclosureTextures(window.sections[i], currPrevSection, i)
        currPrevSection.color(window.frameColor.fillColor);
        currPrevSection.opacity(window.frameColor.fillOpacity);

        // Set up section operation

        self.updateSectionOperation(window.sections[i], currPrevSection, i)

        // Set Up Grille Pattern

        currPrevSection.grilleColor(window.sections[i].grilleColor.fillColor);
        currPrevSection.grilleSize(window.sections[i].grilleSize.name);

        var grillePatternFactory = new windowStore.order.item.preview.viewModel.GrillePatternFactory(self.scale, currPrevSection);
        currPrevSection.grillePattern = grillePatternFactory.getGrillePattern(window.sections[i].grillePattern.name);

        // Set Up SDL Pattern

        currPrevSection.sdlColor(window.sections[i].sdlColor.fillColor);
        currPrevSection.sdlSize(window.sections[i].sdlSize.name);

        var sdlPatternFactory = new windowStore.order.item.preview.viewModel.SdlPatternFactory(self.scale, currPrevSection);
        currPrevSection.sdlPattern = sdlPatternFactory.getSdlPattern(window.sections[i].sdlPattern.name);

        // Set Up Style Lines

        var styleFactory = new windowStore.order.item.preview.viewModel.StyleFactory(self.scale, currPrevSection);
        currPrevSection.stylePattern = styleFactory.getStyle(window.sections[i].style.name);

        // If this is the selected section color it in a little bit

        if (window.sections[i].col === window.selectedSection.col) {
          currPrevSection.overlayOpacity(0.4);
        }

        // Add the section

        frame.sections.push(currPrevSection);
      }
    }

    // Brickmould

    //if (window.brickmouldStyle.name !== "None") {
      
      brickmould = self.brickmould;
      
      // Set up the width of the brickmould

      {
        var brickmouldMeasurement = new windowStore.measurement.measurement().init(
          window.brickmouldStyle.width.sign,
          window.brickmouldStyle.width.whole,
          window.brickmouldStyle.width.numerator,
          window.brickmouldStyle.width.denominator
        );
        var brickmouldDecimal = brickmouldMeasurement.getDecimal();
        brickmould.enclosureWidth(brickmouldDecimal * self.scale());
      }

      // Set brickmould positioning and size

      {
        brickmould.x(self.frame.x() - brickmould.enclosureWidth());
        brickmould.y(self.frame.y() - brickmould.enclosureWidth());
        brickmould.width(self.frame.width() + (2 * self.frame.enclosureWidth()));// + (2 * brickmould.enclosureWidth()));
        brickmould.height(self.frame.height() + (2 * self.frame.enclosureWidth()));// + (2 * brickmould.enclosureWidth()));
      }

      // Set up the brickmoulds enclosure image textures

      {
        brickmould.imageWidth(brickmould.enclosureWidth());
        brickmould.topLeftImage(window.brickmouldStyle.imageTopLeft);
        brickmould.topRightImage(window.brickmouldStyle.imageTopRight);
        brickmould.bottomRightImage(window.brickmouldStyle.imageBottomRight);
        brickmould.bottomLeftImage(window.brickmouldStyle.imageBottomLeft);
        brickmould.topImage(window.brickmouldStyle.imageTop);
        brickmould.rightImage(window.brickmouldStyle.imageRight);
        brickmould.bottomImage(window.brickmouldStyle.imageBottom);
        brickmould.leftImage(window.brickmouldStyle.imageLeft);

        brickmould.topLeftImagePatternId("top-left-brickmould");
        brickmould.topRightImagePatternId("top-right-brickmould");
        brickmould.bottomRightImagePatternId("bottom-right-brickmould");
        brickmould.bottomLeftImagePatternId("bottom-left-brickmould");
        brickmould.leftImagePatternId("left-brickmould");
        brickmould.topImagePatternId("top-brickmould");
        brickmould.rightImagePatternId("right-brickmould");
        brickmould.bottomImagePatternId("bottom-brickmould");

        brickmould.color(window.frameColor.fillColor);
        brickmould.opacity(window.frameColor.fillOpacity);
      }
    //}

    // Set up mullions and add them to the list

    {
      frame.mullions.splice(0, frame.mullions().length);
      if (window.sections.length > 0) {
        for (var i = 1; i < window.sections.length; i++) {
          var curPrevMullion = new windowStore.order.item.preview.viewModel.Mullion();
          var curPrevSection = frame.sections()[i - 1];

          self.updateMullionDimensions(window, curPrevSection, curPrevMullion, mullionWidthDecimal);
          self.updateMullionTextures(window, curPrevMullion, i);

          frame.mullions.push(curPrevMullion);
        }
      }
    }
  };

  self.updateMullionDimensions = function (window, previewSection, previewMullion, mullionWidthDecimal) {
    var mullionIndentTop = new windowStore.measurement.measurement().init(
      window.productLine.mullionIndentTop.sign,
      window.productLine.mullionIndentTop.whole,
      window.productLine.mullionIndentTop.numerator,
      window.productLine.mullionIndentTop.denominator
    );
    var mullionIndentTopDecimal = mullionIndentTop.getDecimal();
    var mullionIndentBottom = new windowStore.measurement.measurement().init(
      window.productLine.mullionIndentBottom.sign,
      window.productLine.mullionIndentBottom.whole,
      window.productLine.mullionIndentBottom.numerator,
      window.productLine.mullionIndentBottom.denominator
    );
    var mullionIndentBottomDecimal = mullionIndentBottom.getDecimal();

    previewMullion.imageWidth(mullionWidthDecimal * self.scale());
    previewMullion.x(previewSection.x() + previewSection.width());
    previewMullion.y(self.frame.y() + (mullionIndentTopDecimal * self.scale())); // (self.frame.enclosureWidth() * 0.35));
    previewMullion.width(mullionWidthDecimal * self.scale());
    previewMullion.height(self.frame.height() + (2 * self.frame.enclosureWidth()) - (mullionIndentTopDecimal * self.scale()) - (mullionIndentBottomDecimal * self.scale()));
  };

  self.updateMullionTextures = function (window, previewMullion, mullionIndex) {
    previewMullion.image(window.productLine.imageMullion);
    previewMullion.imagePatternId("mullion-" + mullionIndex);
    previewMullion.color(window.frameColor.fillColor);
    previewMullion.opacity(window.frameColor.fillOpacity);
  };

  self.updateSectionDimensions = function (windowSection, previewSection, currentSectionX, currentSectionY, mullionTrim, frameTrim) {
    previewSection.x(currentSectionX);
    previewSection.y(currentSectionY);

    var sectionWidthMeasurement = new windowStore.measurement.measurement().init(
      windowSection.width.sign,
      windowSection.width.whole,
      windowSection.width.numerator,
      windowSection.width.denominator
    );
    var sectionHeightMeasurement = new windowStore.measurement.measurement().init(
      windowSection.height.sign,
      windowSection.height.whole,
      windowSection.height.numerator,
      windowSection.height.denominator
    );
    var sectionWidthDecimal = sectionWidthMeasurement.getDecimal();
    var sectionHeightDecimal = sectionHeightMeasurement.getDecimal();

    var sectionWidthToScale = sectionWidthDecimal * self.scale();
    var sectionHeightToScale = sectionHeightDecimal * self.scale();

    previewSection.width(sectionWidthToScale - mullionTrim);
    previewSection.height(sectionHeightToScale - frameTrim);
  };

  self.updateSectionEnclosure = function (windowSection, previewSection) {
    var enclosureWidthMeasurement = new windowStore.measurement.measurement().init(
      windowSection.style.enclosureWidth.sign,
      windowSection.style.enclosureWidth.whole,
      windowSection.style.enclosureWidth.numerator,
      windowSection.style.enclosureWidth.denominator
    );
    var enclosureWidthDecimal = enclosureWidthMeasurement.getDecimal();

    previewSection.style = {
      enclosureWidth: ko.observable({
        sign: ko.observable(windowSection.style.enclosureWidth.sign),
        whole: ko.observable(windowSection.style.enclosureWidth.whole),
        numerator: ko.observable(windowSection.style.enclosureWidth.numerator),
        denominator: ko.observable(windowSection.style.enclosureWidth.denominator)
      })
    };
    previewSection.enclosureWidth(enclosureWidthDecimal * self.scale());
    previewSection.imageWidth(enclosureWidthDecimal * self.scale());
  };

  self.updateSectionEnclosureTextures = function (windowSection, previewSection, sectionIndex) {
    previewSection.topLeftImage(windowSection.style.imageTopLeft);
    previewSection.topRightImage(windowSection.style.imageTopRight);
    previewSection.bottomRightImage(windowSection.style.imageBottomRight);
    previewSection.bottomLeftImage(windowSection.style.imageBottomLeft);
    previewSection.leftImage(windowSection.style.imageLeft);
    previewSection.topImage(windowSection.style.imageTop);
    previewSection.rightImage(windowSection.style.imageRight);
    previewSection.bottomImage(windowSection.style.imageBottom);

    previewSection.topLeftImagePatternId("top-left-section-" + sectionIndex);
    previewSection.topRightImagePatternId("top-right-section-" + sectionIndex);
    previewSection.bottomRightImagePatternId("bottom-right-section-" + sectionIndex);
    previewSection.bottomLeftImagePatternId("bottom-left-section-" + sectionIndex);
    previewSection.leftImagePatternId("left-section-" + sectionIndex);
    previewSection.topImagePatternId("top-section-" + sectionIndex);
    previewSection.rightImagePatternId("right-section-" + sectionIndex);
    previewSection.bottomImagePatternId("bottom-section-" + sectionIndex);
  };

  self.updateSectionOperation = function (windowSection, previewSection, sectionIndex) {
    previewSection.hasHorizontalSlider(windowSection.style.hasHorizontalSlider);
    previewSection.hasVerticalSlider(windowSection.style.hasVerticalSlider);
    previewSection.sliderImage(windowSection.style.sliderImage);
    previewSection.verticalSliderImage(windowSection.style.sliderImage);
    previewSection.sliderImageWidth(2 * self.scale());
    previewSection.horizontalSliderImagePatternId("horizontal-slider-" + sectionIndex);
    previewSection.verticalSliderImagePatternId("vertical-slider-" + sectionIndex);
  };

  self.onSectionClick = function (section) {
    for (var i = 0; i < self.frame.sections().length; i++) {
      self.frame.sections()[i].overlayOpacity(0.0);
    }
    section.overlayOpacity(0.4);

    if (self.sectionSelectedCallback != null) {
      self.sectionSelectedCallback(section);
    }
  };
}

windowStore.order.item.preview.viewModel.GrillePatternFactory = function (scale, section) {
  this.grilleLinesFactory =
    new windowStore.order.item.grillePatterns.GrilleLinesFactory(scale, section);

  this.getGrillePattern = function (patternName) {
    switch (patternName) {
      case "None":
        {
          return {
            grilleLines: this.grilleLinesFactory.createNoneLines().grilleLines()
          };
        }
      case "Ladder":
        {
          return {
            grilleLines: this.grilleLinesFactory.createLadderLines().grilleLines()
          };
        }
      case "Double Ladder":
        {
          return {
            grilleLines: this.grilleLinesFactory.createDoubleLadderLines().grilleLines()
          };
        }
      case "Rectangular":
        {
          return {
            grilleLines: this.grilleLinesFactory.createRectangularLines().grilleLines()
          };
        }
      case "Perimeter":
        {
          return {
            grilleLines: this.grilleLinesFactory.createPerimeterLines().grilleLines()
          };
        }
      case "Double Perimeter":
        {
          return {
            grilleLines: this.grilleLinesFactory.createDoublePerimeterLines().grilleLines()
          };
        }
      case "Empress":
        {
          return {
            grilleLines: this.grilleLinesFactory.createEmpressLines().grilleLines()
          };
        }
      default:
        {
          return {
            grilleLines: this.grilleLinesFactory.createNoneLines().grilleLines()
          };
        }
    };
  };
};

windowStore.order.item.preview.viewModel.SdlPatternFactory = function (scale, section) {
  this.sdlLinesFactory =
    new windowStore.order.item.sdlPatterns.SdlLinesFactory(scale, section);

  this.getSdlPattern = function (patternName) {
    switch (patternName) {
      case "None":
        {
          return {
            sdlLines: this.sdlLinesFactory.createNoneLines().sdlLines()
          };
        }
      case "Colonial":
        {
          return {
            sdlLines: this.sdlLinesFactory.createColonialLines().sdlLines()
          };
        }
      case "Craftsman":
        {
          return {
            sdlLines: this.sdlLinesFactory.createCraftsmanLines().sdlLines()
          };
        }
      case "Heritage":
        {
          return {
            sdlLines: this.sdlLinesFactory.createHeritageLines().sdlLines()
          };
        }
      default:
        {
          return {
            sdlLines: this.sdlLinesFactory.createNoneLines().sdlLines()
          };
        }
    }
  };
};

windowStore.order.item.preview.viewModel.StyleFactory = function (scale, section) {
  this.styleLinesFactory =
    new windowStore.order.item.stylePatterns.StyleLinesFactory(scale, section);

  this.getStyle = function (styleName) {
    switch (styleName) {
      case "Picture":
        {
          return {
            hasStartArrow: ko.observable(false),
            hasEndArrow: ko.observable(false),
            styleLines: this.styleLinesFactory.createPictureLines().styleLines()
          };
        }
      case "Awning":
        {
          return {
            hasStartArrow: ko.observable(false),
            hasEndArrow: ko.observable(false),
            styleLines: this.styleLinesFactory.createAwningLines().styleLines()
          };
        }
      case "Casement":
        {
          return {
            hasStartArrow: ko.observable(false),
            hasEndArrow: ko.observable(false),
            styleLines: this.styleLinesFactory.createCasementLines().styleLines()
          };
        }
      case "Casement - Left":
        {
          return {
            hasStartArrow: ko.observable(false),
            hasEndArrow: ko.observable(false),
            styleLines: this.styleLinesFactory.createCasementLeftLines().styleLines()
          };
        }
      case "Fixed Sash":
        {
          return {
            hasStartArrow: ko.observable(false),
            hasEndArrow: ko.observable(true),
            styleLines: this.styleLinesFactory.createFixedSashLines().styleLines()
          };
        }
      case "Glider":
        {
          return {
            hasStartArrow: ko.observable(false),
            hasEndArrow: ko.observable(true),
            styleLines: this.styleLinesFactory.createGliderLines().styleLines()
          };
        }
      case "Glider - Left":
        {
          return {
            hasStartArrow: ko.observable(true),
            hasEndArrow: ko.observable(false),
            styleLines: this.styleLinesFactory.createGliderLeftLines().styleLines()
          };
        }
      case "Single Hung":
        {
          return {
            hasStartArrow: ko.observable(true),
            hasEndArrow: ko.observable(false),
            styleLines: this.styleLinesFactory.createSingleHungLines().styleLines()
          };
        }
      case "Single Hung - Down":
        {
          return {
            hasStartArrow: ko.observable(false),
            hasEndArrow: ko.observable(true),
            styleLines: this.styleLinesFactory.createSingleHungDownLines().styleLines()
          };
        }
      case "Double Hung":
        {
          return {
            hasStartArrow: ko.observable(true),
            hasEndArrow: ko.observable(true),
            styleLines: this.styleLinesFactory.createDoubleHungLines().styleLines()
          };
        }
      default:
        {
          return {
            hasStartArrow: ko.observable(false),
            hasEndArrow: ko.observable(false),
            styleLines: this.styleLinesFactory.createPictureLines().styleLines()
          };
        }
    }
  };
};
