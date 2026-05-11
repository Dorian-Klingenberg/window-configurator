var windowStore = windowStore || {};
windowStore.order = windowStore.order || {};
windowStore.order.item = windowStore.order.item || {};
windowStore.order.item.grillePatterns = windowStore.order.item.grillePatterns || {};

windowStore.order.item.grillePatterns.GrilleLinesFactory = function (scale, previewSection) {
  var self = this;

  self.scale = scale;
  self.previewSection = previewSection;
  self.convertedSection = {
    top: ko.computed(function () {
      return self.previewSection.y() + self.previewSection.imageWidth();
    }),
    left: ko.computed(function () {
      return self.previewSection.x() + self.previewSection.imageWidth();
    }),
    bottom: ko.computed(function () {
      return self.previewSection.y() + self.previewSection.height() - self.previewSection.imageWidth();
    }),
    right: ko.computed(function () {
      return self.previewSection.x() + self.previewSection.width() - self.previewSection.imageWidth();
    }),
    width: ko.computed(function () {
      return self.previewSection.width() - (2 * self.previewSection.imageWidth());
    }),
    height: ko.computed(function () {
      return self.previewSection.height() - (2 * self.previewSection.imageWidth());
    }),
    section: {
      grilleColor: {
        colorMedium: self.previewSection.grilleColor
      }
    }
  };

  self.mapLines = function (grillePattern) {
    var lines = grillePattern.grilleLines()();

    var res = ko.observableArray();
    for (var i = 0; i < lines.length; i++) {
      res.push(lines[i]);
    }

    return res;
  }

  self.createNoneLines = function () {
    return {
      grilleLines: ko.observableArray()
    };
  };
  self.createLadderLines = function () {
    var grillePattern = new windowStore.order.item.grillePatterns.grilleLadder(self.scale, self.convertedSection);
    return {
      grilleLines: self.mapLines(grillePattern)
    };
  };
  self.createDoubleLadderLines = function () {
    var grillePattern = new windowStore.order.item.grillePatterns.grilleDoubleLadder(self.scale, self.convertedSection);
    return {
      grilleLines: self.mapLines(grillePattern)
    };
  };
  self.createRectangularLines = function () {
    var grillePattern = new windowStore.order.item.grillePatterns.grilleRectangular(self.scale, self.convertedSection);
    return {
      grilleLines: self.mapLines(grillePattern)
    };
  };
  self.createPerimeterLines = function () {
    var grillePattern = new windowStore.order.item.grillePatterns.grillePerimeter(self.scale, self.convertedSection);
    return {
      grilleLines: self.mapLines(grillePattern)
    };
  };
  self.createDoublePerimeterLines = function () {
    var grillePattern = new windowStore.order.item.grillePatterns.grilleDoublePerimeter(self.scale, self.convertedSection);
    return {
      grilleLines: self.mapLines(grillePattern)
    };
  };
  //Empress
  self.createEmpressLines = function () {
    var grillePattern = new windowStore.order.item.grillePatterns.grilleEmpress(self.scale, self.convertedSection);
    return {
      grilleLines: self.mapLines(grillePattern)
    };
  };
};

windowStore.order.item.grillePatterns.grilleNone = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.grilleLines = ko.computed(function () {
    return ko.observableArray([]);
  }, this, {
    deferEvaluation: true
  });

};

windowStore.order.item.grillePatterns.grilleDoublePerimeter = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.grilleLines = ko.computed(function () {
    var res = ko.observableArray([]);

    var outerEdgeDistance = new windowStore.measurement.measurement();
    outerEdgeDistance.init(1, 3, 0, 1);

    var outerEdgeDistanceToScale = outerEdgeDistance.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left() + outerEdgeDistanceToScale;
    var rightPosition = self.previewSection.left() + self.previewSection.width() - outerEdgeDistanceToScale;
    var topPosition = self.previewSection.top() + outerEdgeDistanceToScale;
    var bottomPosition = self.previewSection.top() + self.previewSection.height() - outerEdgeDistanceToScale;

    var line = {
      x1: ko.observable(leftPosition),
      y1: ko.observable(self.previewSection.top()),
      x2: ko.observable(leftPosition),
      y2: ko.observable(self.previewSection.bottom()),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(rightPosition),
      y1: ko.observable(self.previewSection.top()),
      x2: ko.observable(rightPosition),
      y2: ko.observable(self.previewSection.bottom()),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(topPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(topPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(bottomPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(bottomPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    var innerEdgeDistance = new windowStore.measurement.measurement();
    innerEdgeDistance.init(1, 4, 0, 1);

    var innerEdgeDistancetoscale = innerEdgeDistance.getDecimal() * self.scale();

    var innerleftPosition = self.previewSection.left() + innerEdgeDistancetoscale;
    var innerrightPosition = self.previewSection.left() + self.previewSection.width() - innerEdgeDistancetoscale;
    var innertopPosition = self.previewSection.top() + innerEdgeDistancetoscale;
    var innerbottomPosition = self.previewSection.top() + self.previewSection.height() - innerEdgeDistancetoscale;

    var line = {
      x1: ko.observable(innerleftPosition),
      y1: ko.observable(self.previewSection.top()),
      x2: ko.observable(innerleftPosition),
      y2: ko.observable(self.previewSection.bottom()),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(innerrightPosition),
      y1: ko.observable(self.previewSection.top()),
      x2: ko.observable(innerrightPosition),
      y2: ko.observable(self.previewSection.bottom()),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(innertopPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(innertopPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(innerbottomPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(innerbottomPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });
};

windowStore.order.item.grillePatterns.grilleEmpress = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;

  self.grilleLines = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeDistance = new windowStore.measurement.measurement();
    edgeDistance.init(1, 3, 0, 1);
    var edgeDistanceToScale = edgeDistance.getDecimal() * self.scale();

    var centerEdgeDistance = new windowStore.measurement.measurement();
    centerEdgeDistance.init(1, 4, 0, 1);
    var centerEdgeDistanceToScale = centerEdgeDistance.getDecimal() * self.scale();

    var bottomEdgeDistance = new windowStore.measurement.measurement();
    bottomEdgeDistance.init(1, 5, 0, 1);
    var bottomEdgeDistanceToScale = bottomEdgeDistance.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left() + edgeDistanceToScale;
    var rightPosition = self.previewSection.left() + self.previewSection.width() - edgeDistanceToScale;
    var topPosition = self.previewSection.top() + edgeDistanceToScale;
    var topcenterposition = self.previewSection.top() + centerEdgeDistanceToScale;
    var bottomPosition = self.previewSection.top() + bottomEdgeDistanceToScale;
    var verybottomPosition = self.previewSection.top() + self.previewSection.height() - edgeDistanceToScale;

    var line = {
      x1: ko.observable(leftPosition),
      y1: ko.observable(self.previewSection.top()),
      x2: ko.observable(leftPosition),
      y2: ko.observable(self.previewSection.bottom()),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(rightPosition),
      y1: ko.observable(self.previewSection.top()),
      x2: ko.observable(rightPosition),
      y2: ko.observable(self.previewSection.bottom()),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(topPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(topPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(topcenterposition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(topcenterposition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(bottomPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(bottomPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(verybottomPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(verybottomPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });
};

windowStore.order.item.grillePatterns.grilleLadder = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.grilleLines = ko.computed(function () {
    var res = ko.observableArray([]);
    var gridSectionWidth = new windowStore.measurement.measurement();
    gridSectionWidth.init(1, 6, 0, 1);

    var gridSectionWidthToScale = gridSectionWidth.getDecimal() * self.scale();
    var horizontalSectionsInGrid = self.previewSection.width() / gridSectionWidthToScale;

    var horizontalSectionsInGridwhole = Math.floor(horizontalSectionsInGrid);
    var horizontalSectionRemainder = horizontalSectionsInGrid - horizontalSectionsInGridwhole;

    // Position of first vertical grille line based on whats left over
    // from calculating how many sections in the grid
    var firstPercentageFromSectionLeft = horizontalSectionRemainder / 2;
    var firstPosition = self.previewSection.left() + firstPercentageFromSectionLeft * gridSectionWidthToScale;
    var currentPosition = firstPosition;

    var topPosition = gridSectionWidthToScale + self.previewSection.top();

    var line = {
      x1: ko.observable(currentPosition),
      y1: self.previewSection.top,
      x2: ko.observable(currentPosition),
      y2: ko.observable(topPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    for (var i = 0; i < horizontalSectionsInGridwhole; i++) {
      currentPosition += Math.floor(gridSectionWidthToScale);
      var line = {
        x1: ko.observable(currentPosition),
        y1: self.previewSection.top,
        x2: ko.observable(currentPosition),
        y2: ko.observable(topPosition),
        color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
      };
      res.push(line);
    }

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(topPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(topPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });
};

windowStore.order.item.grillePatterns.grilleDoubleLadder = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.grilleLines = ko.computed(function () {
    var res = ko.observableArray([]);
    var gridSectionWidth = new windowStore.measurement.measurement();
    gridSectionWidth.init(1, 6, 0, 1);

    var gridSectionWidthToScale = gridSectionWidth.getDecimal() * self.scale();
    var horizontalSectionsInGrid = self.previewSection.width() / gridSectionWidthToScale;

    var horizontalSectionsInGridwhole = Math.floor(horizontalSectionsInGrid);
    var horizontalSectionRemainder = horizontalSectionsInGrid - horizontalSectionsInGridwhole;

    // Position of first vertical grille line based on whats left over
    // from calculating how many sections in the grid
    var firstPercentageFromSectionLeft = horizontalSectionRemainder / 2;
    var firstPosition = self.previewSection.left() + firstPercentageFromSectionLeft * gridSectionWidthToScale;
    var currentPosition = firstPosition;

    var topPosition = gridSectionWidthToScale + self.previewSection.top();
    var bottomPosition = (gridSectionWidth.getDecimal() * 2 * self.scale()) + self.previewSection.top();

    var line = {
      x1: ko.observable(currentPosition),
      y1: self.previewSection.top,
      x2: ko.observable(currentPosition),
      y2: ko.observable(bottomPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    for (var i = 0; i < horizontalSectionsInGridwhole; i++) {
      currentPosition += Math.floor(gridSectionWidthToScale);
      var line = {
        x1: ko.observable(currentPosition),
        y1: self.previewSection.top,
        x2: ko.observable(currentPosition),
        y2: ko.observable(bottomPosition),
        color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
      };
      res.push(line);
    }

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(topPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(topPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(bottomPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(bottomPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });
};

windowStore.order.item.grillePatterns.grilleRectangular = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.grilleLines = ko.computed(function () {
    var res = ko.observableArray([]);
    var gridSectionWidth = new windowStore.measurement.measurement();
    gridSectionWidth.init(1, 6, 0, 1);

    var gridSectionWidthToScale = gridSectionWidth.getDecimal() * self.scale();
    var horizontalSectionsInGrid = self.previewSection.width() / gridSectionWidthToScale;
    var horizontalSectionsInGridwhole = Math.floor(horizontalSectionsInGrid);
    var horizontalSectionRemainder = horizontalSectionsInGrid - horizontalSectionsInGridwhole;

    // Position of first vertical grille line based on whats left over
    // from calculating how many sections in the grid
    var firstPercentageFromSectionLeft = horizontalSectionRemainder / 2;
    var firstPosition = self.previewSection.left() + firstPercentageFromSectionLeft * gridSectionWidthToScale;
    var currentPosition = firstPosition;

    var topPosition = gridSectionWidthToScale + self.previewSection.top();
    var bottomPosition = (gridSectionWidth.getDecimal() * 2 * self.scale()) + self.previewSection.top();

    var line = {
      x1: ko.observable(currentPosition),
      y1: self.previewSection.top,
      x2: ko.observable(currentPosition),
      y2: self.previewSection.bottom,
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    for (var i = 0; i < horizontalSectionsInGridwhole; i++) {
      currentPosition += Math.floor(gridSectionWidthToScale);
      var line = {
        x1: ko.observable(currentPosition),
        y1: self.previewSection.top,
        x2: ko.observable(currentPosition),
        y2: self.previewSection.bottom,
        color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
      };
      res.push(line);
    }

    // Position of first horizontal grille line based on whats left over
    var verticalSectionsInGrid = self.previewSection.height() / gridSectionWidthToScale;
    var verticalSectionsInGridWhole = Math.floor(verticalSectionsInGrid);
    var verticalSectionRemainder = verticalSectionsInGrid - verticalSectionsInGridWhole;
    var firstPercentageFromSectionTop = verticalSectionRemainder / 2;

    firstPosition = self.previewSection.top() + firstPercentageFromSectionTop * gridSectionWidthToScale;
    currentPosition = firstPosition;

    line = {
      x1: self.previewSection.left,
      y1: ko.observable(currentPosition),
      x2: self.previewSection.right,
      y2: ko.observable(currentPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    for (var i = 0; i < verticalSectionsInGridWhole; i++) {
      currentPosition += Math.floor(gridSectionWidthToScale);
      var line = {
        x1: self.previewSection.left,
        y1: ko.observable(currentPosition),
        x2: self.previewSection.right,
        y2: ko.observable(currentPosition),
        color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
      };
      res.push(line);
    }

    return res;
  }, this, {
    deferEvaluation: true
  });
};

windowStore.order.item.grillePatterns.grillePerimeter = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.grilleLines = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeDistance = new windowStore.measurement.measurement();
    edgeDistance.init(1, 3, 0, 1);

    var edgeDistanceToScale = edgeDistance.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left() + edgeDistanceToScale;
    var rightPosition = self.previewSection.left() + self.previewSection.width() - edgeDistanceToScale;
    var topPosition = self.previewSection.top() + edgeDistanceToScale;
    var bottomPosition = self.previewSection.top() + self.previewSection.height() - edgeDistanceToScale;

    var line = {
      x1: ko.observable(leftPosition),
      y1: ko.observable(self.previewSection.top()),
      x2: ko.observable(leftPosition),
      y2: ko.observable(self.previewSection.bottom()),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(rightPosition),
      y1: ko.observable(self.previewSection.top()),
      x2: ko.observable(rightPosition),
      y2: ko.observable(self.previewSection.bottom()),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(topPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(topPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(bottomPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(bottomPosition),
      color: ko.observable(self.previewSection.section.grilleColor.colorMedium())
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });
};

////////////////////////////////////////////////////////////////////////////////////////////////

windowStore.order.item.stylePatterns = windowStore.order.item.stylePatterns || {}

windowStore.order.item.stylePatterns.StyleLinesFactory = function (scale, previewSection) {
  var self = this;

  self.scale = scale;
  self.previewSection = previewSection;
  self.convertedSection = {
    top: ko.computed(function () {
      return self.previewSection.y();
    }),
    left: ko.computed(function () {
      return self.previewSection.x();
    }),
    bottom: ko.computed(function () {
      return self.previewSection.y() + self.previewSection.height();
    }),
    right: ko.computed(function () {
      return self.previewSection.x() + self.previewSection.width();
    }),
    width: ko.computed(function () {
      return self.previewSection.width();
    }),
    height: ko.computed(function () {
      return self.previewSection.height();
    }),
    edgeWidth: ko.computed(function () {
      return self.previewSection.style.enclosureWidth();
    })
  };

  self.mapLines = function (stylePattern) {
    var lines = stylePattern.styleLines()();

    var res = ko.observableArray();
    for (var i = 0; i < lines.length; i++) {
      res.push(lines[i]);
    }

    return res;
  }

  self.createPictureLines = function () {
    return {
      styleLines: ko.observableArray()
    };
  };
  self.createAwningLines = function () {
    var stylePattern = new windowStore.order.item.stylePatterns.operationalStyleAwening(self.scale, self.convertedSection);
    return {
      styleLines: self.mapLines(stylePattern)
    };
  };
  self.createCasementLines = function () {
    var stylePattern = new windowStore.order.item.stylePatterns.operationalStyleCasement(self.scale, self.convertedSection);
    return {
      styleLines: self.mapLines(stylePattern)
    };
  };
  self.createCasementLeftLines = function () {
    var stylePattern = new windowStore.order.item.stylePatterns.operationalStyleCasementleft(self.scale, self.convertedSection);
    return {
      styleLines: self.mapLines(stylePattern)
    };
  };
  self.createFixedSashLines = function () {
    var stylePattern = new windowStore.order.item.stylePatterns.operationalStyleFixedSash(self.scale, self.convertedSection);
    return {
      styleLines: self.mapLines(stylePattern)
    };
  };
  self.createGliderLines = function () {
    var stylePattern = new windowStore.order.item.stylePatterns.operationalStyleGlider(self.scale, self.convertedSection);
    return {
      styleLines: self.mapLines(stylePattern)
    };
  };
  self.createGliderLeftLines = function () {
    var stylePattern = new windowStore.order.item.stylePatterns.operationalStyleGliderLeft(self.scale, self.convertedSection);
    return {
      styleLines: self.mapLines(stylePattern)
    };
  };
  self.createSingleHungLines = function () {
    var stylePattern = new windowStore.order.item.stylePatterns.operationalStyleSingleHung(self.scale, self.convertedSection);
    return {
      styleLines: self.mapLines(stylePattern)
    };
  };
  self.createSingleHungDownLines = function () {
    var stylePattern = new windowStore.order.item.stylePatterns.operationalStyleSingleHungDown(self.scale, self.convertedSection);
    return {
      styleLines: self.mapLines(stylePattern)
    };
  };
  self.createDoubleHungLines = function () {
    var stylePattern = new windowStore.order.item.stylePatterns.operationalStyleDoubleHung(self.scale, self.convertedSection);
    return {
      styleLines: self.mapLines(stylePattern)
    };
  };
};

windowStore.order.item.stylePatterns.operationalStyleNone = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;

  self.styleRects = ko.computed(function () {
    return ko.observableArray([]);
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    return ko.observableArray([]);
  }, this, {
    deferEvaluation: true
  });
}

windowStore.order.item.stylePatterns.operationalStyleAweningUp = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );
    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left();
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top();
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(rightPosition - leftPosition),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(rightPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(bottomPosition),
      width: ko.observable(rightPosition - leftPosition + edgeWidthToScale),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left() + edgeWidthToScale;
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top() + edgeWidthToScale;
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var line = {
      x1: ko.observable(leftPosition),
      y1: ko.observable(topPosition),
      x2: ko.observable(rightPosition + ((leftPosition - rightPosition) / 2)),
      y2: ko.observable(bottomPosition)
    };
    res.push(line);

    line = {
      x1: ko.observable(rightPosition + ((leftPosition - rightPosition) / 2)),
      y1: ko.observable(bottomPosition),
      x2: ko.observable(rightPosition),
      y2: ko.observable(topPosition)
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });

}

windowStore.order.item.stylePatterns.operationalStyleAwening = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;

  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left();
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top();
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(rightPosition - leftPosition),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(rightPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(bottomPosition),
      width: ko.observable(rightPosition - leftPosition + edgeWidthToScale),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left() + edgeWidthToScale;
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top() + edgeWidthToScale;
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var line = {
      x1: ko.observable(leftPosition),
      y1: ko.observable(bottomPosition),
      x2: ko.observable(rightPosition + ((leftPosition - rightPosition) / 2)),
      y2: ko.observable(topPosition)
    };
    res.push(line);

    line = {
      x1: ko.observable(rightPosition + ((leftPosition - rightPosition) / 2)),
      y1: ko.observable(topPosition),
      x2: ko.observable(rightPosition),
      y2: ko.observable(bottomPosition)
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });

}

windowStore.order.item.stylePatterns.operationalStyleCasement = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;

  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left();
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top();
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(rightPosition - leftPosition),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(rightPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(bottomPosition),
      width: ko.observable(rightPosition - leftPosition + edgeWidthToScale),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left() + edgeWidthToScale;
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top() + edgeWidthToScale;
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var line = {
      x1: ko.observable(leftPosition),
      y1: ko.observable(topPosition),
      x2: ko.observable(rightPosition),
      y2: ko.observable(topPosition + ((bottomPosition - topPosition) / 2))
    };
    res.push(line);

    line = {
      x1: ko.observable(rightPosition),
      y1: ko.observable(topPosition + ((bottomPosition - topPosition) / 2)),
      x2: ko.observable(leftPosition),
      y2: ko.observable(bottomPosition)
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });

}

windowStore.order.item.stylePatterns.operationalStyleCasementleft = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left();
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top();
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(rightPosition - leftPosition),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(rightPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(bottomPosition),
      width: ko.observable(rightPosition - leftPosition + edgeWidthToScale),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left() + edgeWidthToScale;
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top() + edgeWidthToScale;
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var line = {
      x1: ko.observable(rightPosition),
      y1: ko.observable(topPosition),
      x2: ko.observable(leftPosition),
      y2: ko.observable(topPosition + ((bottomPosition - topPosition) / 2))
    };
    res.push(line);

    line = {
      x1: ko.observable(leftPosition),
      y1: ko.observable(topPosition + ((bottomPosition - topPosition) / 2)),
      x2: ko.observable(rightPosition),
      y2: ko.observable(bottomPosition)
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });

}

windowStore.order.item.stylePatterns.operationalStyleFixedSash = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left();
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top();
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(rightPosition - leftPosition),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(rightPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(bottomPosition),
      width: ko.observable(rightPosition - leftPosition + edgeWidthToScale),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    return ko.observableArray([]);
  }, this, {
    deferEvaluation: true
  });
}

// TODO: Arrow tips
windowStore.order.item.stylePatterns.operationalStyleGlider = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left();
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top();
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(rightPosition - leftPosition),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(rightPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(bottomPosition),
      width: ko.observable(rightPosition - leftPosition + edgeWidthToScale),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    var crossBeamWidth = new windowStore.measurement.measurement();
    crossBeamWidth.init(1, 2, 0, 1);
    var crossBeamWidthToScale = crossBeamWidth.getDecimal() * self.scale();

    rect = {
      x: ko.observable((leftPosition + ((rightPosition - leftPosition) / 2)) + (crossBeamWidthToScale / 2)),
      y: ko.observable(topPosition),
      width: ko.observable(crossBeamWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    res = ko.observableArray([]);

    var leftxPosition = self.previewSection.left() + (self.previewSection.width() / 4);
    var rightxPosition = self.previewSection.left() + ((self.previewSection.width() / 4) * 3);
    var yposition = self.previewSection.top() + (self.previewSection.height() / 2);

    var line = {
      x1: ko.observable(leftxPosition),
      y1: ko.observable(yposition),
      x2: ko.observable(rightxPosition),
      y2: ko.observable(yposition)
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });

}

// TODO: Arrow tips
windowStore.order.item.stylePatterns.operationalStyleGliderLeft = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left();
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top();
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(rightPosition - leftPosition),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(rightPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(bottomPosition),
      width: ko.observable(rightPosition - leftPosition + edgeWidthToScale),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    var crossBeamWidth = new windowStore.measurement.measurement();
    crossBeamWidth.init(1, 2, 0, 1);
    var crossBeamWidthToScale = crossBeamWidth.getDecimal() * self.scale();

    rect = {
      x: ko.observable((leftPosition + ((rightPosition - leftPosition) / 2)) + (crossBeamWidthToScale / 2)),
      y: ko.observable(topPosition),
      width: ko.observable(crossBeamWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    res = ko.observableArray([]);

    var leftxPosition = self.previewSection.left() + (self.previewSection.width() / 4);
    var rightxPosition = self.previewSection.left() + ((self.previewSection.width() / 4) * 3);
    var yposition = self.previewSection.top() + (self.previewSection.height() / 2);

    var line = {
      x1: ko.observable(leftxPosition),
      y1: ko.observable(yposition),
      x2: ko.observable(rightxPosition),
      y2: ko.observable(yposition)
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });

}

windowStore.order.item.stylePatterns.operationalStylePicture = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var edgeWidthToScale = edgeWidth.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left();
    var rightPosition = self.previewSection.right() - edgeWidthToScale;
    var topPosition = self.previewSection.top();
    var bottomPosition = self.previewSection.bottom() - edgeWidthToScale;

    var rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(topPosition),
      width: ko.observable(rightPosition - leftPosition),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(rightPosition),
      y: ko.observable(topPosition),
      width: ko.observable(edgeWidthToScale),
      height: ko.observable(bottomPosition - topPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(leftPosition),
      y: ko.observable(bottomPosition),
      width: ko.observable(rightPosition - leftPosition + edgeWidthToScale),
      height: ko.observable(edgeWidthToScale)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    return ko.observableArray([]);
  }, this, {
    deferEvaluation: true
  });

}

windowStore.order.item.stylePatterns.operationalStyleSingleHung = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var upperEdgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var upperEdgeWidthToScale = upperEdgeWidth.getDecimal() * self.scale();

    var upperleftPosition = self.previewSection.left();
    var upperrightPosition = self.previewSection.right() - upperEdgeWidthToScale;
    var uppertopPosition = self.previewSection.top();
    var upperbottomPosition = self.previewSection.top() + ((self.previewSection.bottom() - self.previewSection.top()) / 2);

    var rect = {
      x: ko.observable(upperleftPosition),
      y: ko.observable(uppertopPosition),
      width: ko.observable(upperEdgeWidthToScale),
      height: ko.observable(upperbottomPosition - uppertopPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(upperleftPosition),
      y: ko.observable(uppertopPosition),
      width: ko.observable(upperrightPosition - upperleftPosition),
      height: ko.observable(upperEdgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(upperrightPosition),
      y: ko.observable(uppertopPosition),
      width: ko.observable(upperEdgeWidthToScale),
      height: ko.observable(upperbottomPosition - uppertopPosition)
    };
    res.push(rect);

    var lowerEdgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var lowerEdgeWidthToScale = lowerEdgeWidth.getDecimal() * self.scale();

    var lowerLeftPosition = self.previewSection.left();
    var lowerRightPosition = self.previewSection.right() - lowerEdgeWidthToScale;
    var lowerTopPosition = self.previewSection.top() + ((self.previewSection.bottom() - self.previewSection.top()) / 2);
    var lowerBottomPosition = self.previewSection.bottom() - lowerEdgeWidthToScale;

    rect = {
      x: ko.observable(lowerLeftPosition),
      y: ko.observable(lowerTopPosition),
      width: ko.observable(lowerEdgeWidthToScale),
      height: ko.observable(lowerBottomPosition - lowerTopPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(lowerRightPosition),
      y: ko.observable(lowerTopPosition),
      width: ko.observable(lowerEdgeWidthToScale),
      height: ko.observable(lowerBottomPosition - lowerTopPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(lowerLeftPosition),
      y: ko.observable(lowerBottomPosition),
      width: ko.observable(lowerRightPosition - lowerLeftPosition + lowerEdgeWidthToScale),
      height: ko.observable(lowerEdgeWidthToScale)
    };
    res.push(rect);

    var crossBeamWidth = new windowStore.measurement.measurement();
    crossBeamWidth.init(1, 2, 0, 1);
    var crossBeamWidthToScale = crossBeamWidth.getDecimal() * self.scale();

    rect = {
      x: ko.observable(lowerLeftPosition),
      y: ko.observable(lowerTopPosition - (crossBeamWidthToScale / 2)),
      width: ko.observable(lowerRightPosition - lowerLeftPosition + upperEdgeWidthToScale),
      height: ko.observable(crossBeamWidthToScale)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    res = ko.observableArray([]);

    var xPosition = self.previewSection.left() + (self.previewSection.width() / 2);
    var upperYPosition = self.previewSection.top() + (self.previewSection.height() / 4);
    var lowerYPosition = self.previewSection.top() + ((self.previewSection.height() / 4) * 3);

    var line = {
      x1: ko.observable(xPosition),
      y1: ko.observable(upperYPosition),
      x2: ko.observable(xPosition),
      y2: ko.observable(lowerYPosition)
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });

}

// TODO: Arrow tips
windowStore.order.item.stylePatterns.operationalStyleSingleHungDown = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var upperEdgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var upperEdgeWidthToScale = upperEdgeWidth.getDecimal() * self.scale();

    var upperleftPosition = self.previewSection.left();
    var upperrightPosition = self.previewSection.right() - upperEdgeWidthToScale;
    var uppertopPosition = self.previewSection.top();
    var upperbottomPosition = self.previewSection.top() + ((self.previewSection.bottom() - self.previewSection.top()) / 2);

    var rect = {
      x: ko.observable(upperleftPosition),
      y: ko.observable(uppertopPosition),
      width: ko.observable(upperEdgeWidthToScale),
      height: ko.observable(upperbottomPosition - uppertopPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(upperleftPosition),
      y: ko.observable(uppertopPosition),
      width: ko.observable(upperrightPosition - upperleftPosition),
      height: ko.observable(upperEdgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(upperrightPosition),
      y: ko.observable(uppertopPosition),
      width: ko.observable(upperEdgeWidthToScale),
      height: ko.observable(upperbottomPosition - uppertopPosition)
    };
    res.push(rect);

    var lowerEdgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var lowerEdgeWidthToScale = lowerEdgeWidth.getDecimal() * self.scale();

    var lowerLeftPosition = self.previewSection.left();
    var lowerRightPosition = self.previewSection.right() - lowerEdgeWidthToScale;
    var lowerTopPosition = self.previewSection.top() + ((self.previewSection.bottom() - self.previewSection.top()) / 2);
    var lowerBottomPosition = self.previewSection.bottom() - lowerEdgeWidthToScale;

    rect = {
      x: ko.observable(lowerLeftPosition),
      y: ko.observable(lowerTopPosition),
      width: ko.observable(lowerEdgeWidthToScale),
      height: ko.observable(lowerBottomPosition - lowerTopPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(lowerRightPosition),
      y: ko.observable(lowerTopPosition),
      width: ko.observable(lowerEdgeWidthToScale),
      height: ko.observable(lowerBottomPosition - lowerTopPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(lowerLeftPosition),
      y: ko.observable(lowerBottomPosition),
      width: ko.observable(lowerRightPosition - lowerLeftPosition + lowerEdgeWidthToScale),
      height: ko.observable(lowerEdgeWidthToScale)
    };
    res.push(rect);

    var crossBeamWidth = new windowStore.measurement.measurement();
    crossBeamWidth.init(1, 2, 0, 1);
    var crossBeamWidthToScale = crossBeamWidth.getDecimal() * self.scale();

    rect = {
      x: ko.observable(lowerLeftPosition),
      y: ko.observable(lowerTopPosition - (crossBeamWidthToScale / 2)),
      width: ko.observable(lowerRightPosition - lowerLeftPosition + upperEdgeWidthToScale),
      height: ko.observable(crossBeamWidthToScale)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    res = ko.observableArray([]);

    var xPosition = self.previewSection.left() + (self.previewSection.width() / 2);
    var upperYPosition = self.previewSection.top() + (self.previewSection.height() / 4);
    var lowerYPosition = self.previewSection.top() + ((self.previewSection.height() / 4) * 3);

    var line = {
      x1: ko.observable(xPosition),
      y1: ko.observable(upperYPosition),
      x2: ko.observable(xPosition),
      y2: ko.observable(lowerYPosition)
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });

}

windowStore.order.item.stylePatterns.operationalStyleDoubleHung = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.styleRects = ko.computed(function () {
    var res = ko.observableArray([]);

    var upperEdgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var upperEdgeWidthToScale = upperEdgeWidth.getDecimal() * self.scale();

    var upperleftPosition = self.previewSection.left();
    var upperrightPosition = self.previewSection.right() - upperEdgeWidthToScale;
    var uppertopPosition = self.previewSection.top();
    var upperbottomPosition = self.previewSection.top() + ((self.previewSection.bottom() - self.previewSection.top()) / 2);

    var rect = {
      x: ko.observable(upperleftPosition),
      y: ko.observable(uppertopPosition),
      width: ko.observable(upperEdgeWidthToScale),
      height: ko.observable(upperbottomPosition - uppertopPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(upperleftPosition),
      y: ko.observable(uppertopPosition),
      width: ko.observable(upperrightPosition - upperleftPosition),
      height: ko.observable(upperEdgeWidthToScale)
    };
    res.push(rect);

    rect = {
      x: ko.observable(upperrightPosition),
      y: ko.observable(uppertopPosition),
      width: ko.observable(upperEdgeWidthToScale),
      height: ko.observable(upperbottomPosition - uppertopPosition)
    };
    res.push(rect);

    var lowerEdgeWidth = new windowStore.measurement.measurement();
    edgeWidth.init(
      previewSection.edgeWidth().sign(),
      previewSection.edgeWidth().whole(),
      previewSection.edgeWidth().numerator(),
      previewSection.edgeWidth().denominator()
    );

    var lowerEdgeWidthToScale = lowerEdgeWidth.getDecimal() * self.scale();

    var lowerLeftPosition = self.previewSection.left();
    var lowerRightPosition = self.previewSection.right() - lowerEdgeWidthToScale;
    var lowerTopPosition = self.previewSection.top() + ((self.previewSection.bottom() - self.previewSection.top()) / 2);
    var lowerBottomPosition = self.previewSection.bottom() - lowerEdgeWidthToScale;

    rect = {
      x: ko.observable(lowerLeftPosition),
      y: ko.observable(lowerTopPosition),
      width: ko.observable(lowerEdgeWidthToScale),
      height: ko.observable(lowerBottomPosition - lowerTopPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(lowerRightPosition),
      y: ko.observable(lowerTopPosition),
      width: ko.observable(lowerEdgeWidthToScale),
      height: ko.observable(lowerBottomPosition - lowerTopPosition)
    };
    res.push(rect);

    rect = {
      x: ko.observable(lowerLeftPosition),
      y: ko.observable(lowerBottomPosition),
      width: ko.observable(lowerRightPosition - lowerLeftPosition + lowerEdgeWidthToScale),
      height: ko.observable(lowerEdgeWidthToScale)
    };
    res.push(rect);

    var crossBeamWidth = new windowStore.measurement.measurement();
    crossBeamWidth.init(1, 2, 0, 1);
    var crossBeamWidthToScale = crossBeamWidth.getDecimal() * self.scale();

    rect = {
      x: ko.observable(lowerLeftPosition),
      y: ko.observable(lowerTopPosition - (crossBeamWidthToScale / 2)),
      width: ko.observable(lowerRightPosition - lowerLeftPosition + upperEdgeWidthToScale),
      height: ko.observable(crossBeamWidthToScale)
    };
    res.push(rect);

    return res;
  }, this, {
    deferEvaluation: true
  });

  self.styleLines = ko.computed(function () {
    res = ko.observableArray([]);

    var xPosition = self.previewSection.left() + (self.previewSection.width() / 2);
    var upperYPosition = self.previewSection.top() + (self.previewSection.height() / 4);
    var lowerYPosition = self.previewSection.top() + ((self.previewSection.height() / 4) * 3);

    var line = {
      x1: ko.observable(xPosition),
      y1: ko.observable(upperYPosition),
      x2: ko.observable(xPosition),
      y2: ko.observable(lowerYPosition)
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });
}

////////////////////////////////////////////////////////////////////////////////////////////////

windowStore.order.item.sdlPatterns = windowStore.order.item.sdlPatterns || {}

windowStore.order.item.sdlPatterns.SdlLinesFactory = function (scale, previewSection) {
  var self = this;

  self.scale = scale;
  self.previewSection = previewSection;
  self.convertedSection = {
    top: ko.computed(function () {
      return self.previewSection.y() + self.previewSection.imageWidth();
    }),
    left: ko.computed(function () {
      return self.previewSection.x() + self.previewSection.imageWidth();
    }),
    bottom: ko.computed(function () {
      return self.previewSection.y() + self.previewSection.height() - self.previewSection.imageWidth();
    }),
    right: ko.computed(function () {
      return self.previewSection.x() + self.previewSection.width() - self.previewSection.imageWidth();
    }),
    width: ko.computed(function () {
      return self.previewSection.width() - (2 * self.previewSection.imageWidth());
    }),
    height: ko.computed(function () {
      return self.previewSection.height() - (2 * self.previewSection.imageWidth());
    }),
    section: {
      sdlColor: {
        colorMedium: self.previewSection.sdlColor
      }
    }
    //sdlWidth: ko.observable(previewSection.sdlWidth())
  };

  self.mapLines = function (sdlPattern) {
    var lines = sdlPattern.sdlLines()();

    var res = ko.observableArray();
    for (var i = 0; i < lines.length; i++) {
      res.push(lines[i]);
    }

    return res;
  }

  self.createNoneLines = function () {
    return {
      sdlLines: ko.observableArray()
    };
  };
  self.createColonialLines = function () {
    var sdlPattern = new windowStore.order.item.sdlPatterns.sdlColonial(self.scale, self.convertedSection);
    return {
      sdlLines: self.mapLines(sdlPattern)
    };
  };
  self.createCraftsmanLines = function () {
    var sdlPattern = new windowStore.order.item.sdlPatterns.sdlCraftsman(self.scale, self.convertedSection);
    return {
      sdlLines: self.mapLines(sdlPattern)
    };
  };
  self.createHeritageLines = function () {
    var sdlPattern = new windowStore.order.item.sdlPatterns.sdlHeritage(self.scale, self.convertedSection);
    return {
      sdlLines: self.mapLines(sdlPattern)
    };
  };
};

windowStore.order.item.sdlPatterns.sdlNone = function (scale, previewSection) {
  var self = this;

  self.section = previewSection;
  self.scale = scale;
  self.sdlLines = ko.computed(function () {
    return ko.observableArray([]);
  }, this, {
    deferEvaluation: true
  });

}

windowStore.order.item.sdlPatterns.sdlColonial = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.sdlLines = ko.computed(function () {
    var res = ko.observableArray([]);
    var gridSectionWidth = new windowStore.measurement.measurement();
    gridSectionWidth.init(1, 6, 0, 1)

    var gridSectionWidthToScale = gridSectionWidth.getDecimal() * self.scale();
    var horizontalSectionsinGrid = self.previewSection.width() / gridSectionWidthToScale;

    var horizontalSectionsinGridwhole = Math.floor(horizontalSectionsinGrid);
    var horizontalsectionremainder = horizontalSectionsinGrid - horizontalSectionsinGridwhole;

    // Position of first vertical grille line based on whats left over
    // from calculating how many sections in the grid
    var firstPercentageFromSectionLeft = horizontalsectionremainder / 2;
    var firstPosition = self.previewSection.left() + firstPercentageFromSectionLeft * gridSectionWidthToScale;
    var currentPosition = firstPosition;

    var topPosition = gridSectionWidthToScale + self.previewSection.top();

    var line = {
      x1: ko.observable(currentPosition),
      y1: self.previewSection.top,
      x2: ko.observable(currentPosition),
      y2: ko.observable(topPosition),
      color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
    };
    res.push(line);

    for (var i = 0; i < horizontalSectionsinGridwhole; i++) {
      currentPosition += Math.floor(gridSectionWidthToScale);
      var line = {
        x1: ko.observable(currentPosition),
        y1: self.previewSection.top,
        x2: ko.observable(currentPosition),
        y2: ko.observable(topPosition),
        color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
      };
      res.push(line);
    }

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(topPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(topPosition),
      color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });
}

windowStore.order.item.sdlPatterns.sdlCraftsman = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.sdlLines = ko.computed(function () {
    var res = ko.observableArray([]);
    var gridSectionWidth = new windowStore.measurement.measurement();
    gridSectionWidth.init(1, 6, 0, 1);

    var gridSectionWidthToScale = gridSectionWidth.getDecimal() * self.scale();
    var horizontalSectionsinGrid = self.previewSection.width() / gridSectionWidthToScale;
    var horizontalSectionsinGridwhole = Math.floor(horizontalSectionsinGrid);
    var horizontalsectionremainder = horizontalSectionsinGrid - horizontalSectionsinGridwhole;

    // Position of first vertical grille line based on whats left over
    // from calculating how many sections in the grid
    var firstPercentageFromSectionLeft = horizontalsectionremainder / 2;
    var firstPosition = self.previewSection.left() + firstPercentageFromSectionLeft * gridSectionWidthToScale;
    var currentPosition = firstPosition;

    var topPosition = gridSectionWidthToScale + self.previewSection.top();
    var bottomPosition = (gridSectionWidth.getDecimal() * 2 * self.scale()) + self.previewSection.top();

    var line = {
      x1: ko.observable(currentPosition),
      y1: self.previewSection.top,
      x2: ko.observable(currentPosition),
      y2: self.previewSection.bottom,
      color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
    };
    res.push(line);

    for (var i = 0; i < horizontalSectionsinGridwhole; i++) {
      currentPosition += Math.floor(gridSectionWidthToScale);
      var line = {
        x1: ko.observable(currentPosition),
        y1: self.previewSection.top,
        x2: ko.observable(currentPosition),
        y2: self.previewSection.bottom,
        color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
      };
      res.push(line);
    }

    // Position of first horizontal grille line based on whats left over
    var verticalSectionsInGrid = self.previewSection.height() / gridSectionWidthToScale;
    var verticalSectionsInGridWhole = Math.floor(verticalSectionsInGrid);
    var verticalSectionRemainder = verticalSectionsInGrid - verticalSectionsInGridWhole;
    var firstPercentageFromSectionTop = verticalSectionRemainder / 2;

    firstPosition = self.previewSection.top() + firstPercentageFromSectionTop * gridSectionWidthToScale;
    currentPosition = firstPosition;

    line = {
      x1: self.previewSection.left,
      y1: ko.observable(currentPosition),
      x2: self.previewSection.right,
      y2: ko.observable(currentPosition),
      color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
    };
    res.push(line);

    for (var i = 0; i < verticalSectionsInGridWhole; i++) {
      currentPosition += Math.floor(gridSectionWidthToScale);
      var line = {
        x1: self.previewSection.left,
        y1: ko.observable(currentPosition),
        x2: self.previewSection.right,
        y2: ko.observable(currentPosition),
        color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
      };
      res.push(line);
    }

    return res;
  }, this, {
    deferEvaluation: true
  });
}

windowStore.order.item.sdlPatterns.sdlHeritage = function (scale, previewSection) {
  var self = this;

  self.previewSection = previewSection;
  self.scale = scale;
  self.sdlLines = ko.computed(function () {
    var res = ko.observableArray([]);

    var edgeDistance = new windowStore.measurement.measurement();
    edgeDistance.init(1, 3, 0, 1);
    var edgeDistanceToScale = edgeDistance.getDecimal() * self.scale();

    var leftPosition = self.previewSection.left() + edgeDistanceToScale;
    var rightPosition = self.previewSection.left() + self.previewSection.width() - edgeDistanceToScale;
    var topPosition = self.previewSection.top() + edgeDistanceToScale;
    var bottomPosition = self.previewSection.top() + self.previewSection.height() - edgeDistanceToScale;

    var line = {
      x1: ko.observable(leftPosition),
      y1: ko.observable(self.previewSection.top()),
      x2: ko.observable(leftPosition),
      y2: ko.observable(self.previewSection.bottom()),
      color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(rightPosition),
      y1: ko.observable(self.previewSection.top()),
      x2: ko.observable(rightPosition),
      y2: ko.observable(self.previewSection.bottom()),
      color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(topPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(topPosition),
      color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
    };
    res.push(line);

    line = {
      x1: ko.observable(self.previewSection.left()),
      y1: ko.observable(bottomPosition),
      x2: ko.observable(self.previewSection.right()),
      y2: ko.observable(bottomPosition),
      color: ko.observable(self.previewSection.section.sdlColor.colorMedium())
    };
    res.push(line);

    return res;
  }, this, {
    deferEvaluation: true
  });
}
