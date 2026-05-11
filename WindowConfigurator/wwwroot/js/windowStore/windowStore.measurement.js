var windowStore = windowStore || {};
windowStore.measurement = windowStore.measurement || {};

windowStore.measurement.errorMessages = function () {
  return {
    validation: {
      parsing: {
        clear: function () {
          this.hasErrors(false);
          this.message();
        },
        hasErrors: ko.observable(false),
        message: ko.observable()
      }
    },
  };
};

windowStore.measurement.measurementValidator = function () {
  var self = this;

  self.WHOLE_ONLY = "whole-only";
  self.FRACTION_ONLY = "fraction-only";
  self.WHOLE_AND_FRACTION = "whole-and-fraction";
  self.PARTIALLY_MATCHED = "partially-matched";

  self.isWholeOnly = function (measurementtext) {
    var signWholePatt = "^([+-])?(\\d+)$";
    var signWholeSpacePatt = "^([+-])?(\\d+)([ ])?$";

    if (measurementtext.match(signWholePatt))
      return true;
    if (measurementtext.match(signWholeSpacePatt))
      return true;

    return false;
  }

  self.isFractionOnly = function (measurementtext) {
    var signNumDenomPatt = "^([+-])?\\d+[\/]\\d+$";

    if (measurementtext.match(signNumDenomPatt))
      return true;

    return false;
  }

  self.isWholeAndFraction = function (measurementtext) {
    var signWholeNumDenomPatt = "^([+-])?(\\d+)([ ]\\d+[\/]\\d+)?$";

    if (measurementtext.match(signWholeNumDenomPatt))
      return true;

    return false;
  }

  self.isPartiallyMatched = function (measurementtext) {
    // Incomplete number patterns
    var signPatt = "^([+-])?$";
    var signWholeNumPatt = "^([+-])?(\\d+)([ ]\\d+)?$";
    var signWholeNumVinPatt = "^([+-])?(\\d+)([ ]\\d+[\/])?$";
    var signNumVinPatt = "^([+-])?\\d+[\/]$";

    if (measurementtext.match(signPatt))
      return true;
    if (measurementtext.match(signWholeNumPatt))
      return true;
    if (measurementtext.match(signWholeNumVinPatt))
      return true;
    if (measurementtext.match(signNumVinPatt))
      return true;

    return false;
  }

  self.isPercentage = function (measurementtext) {
    var percentPatt = "^(\\d+)([.]\\d+)?(%)$";

    if (measurementtext.match(percentPatt)) {
      return true;
    }

    return false;
  };

  self.stringNotParsable = function (measurementText) {
    if (self.isWholeOnly(measurementText)) {
      return false;
    } else if (self.isFractionOnly(measurementText)) {
      return false;
    } else if (self.isWholeAndFraction(measurementText)) {
      return false;
    } else if (self.isPartiallyMatched(measurementText)) {
      return true;
    } else {
      return true;
    }
  }

  self.checkTextFormat = function (measurementText) {
    var res = {
      hasErrors: ko.observable(false),
      matchesTo: "",
      isPartialMatch: false,
      errorMessage: {},
      isWellFormed: true,
      reason: "",
    };

    if (self.isWholeOnly(measurementText)) {
      res.matchesTo = "whole-only";
    } else if (self.isFractionOnly(measurementText)) {
      res.matchesTo = "fraction-only";
    } else if (self.isWholeAndFraction(measurementText)) {
      res.matchesTo = "whole-and-fraction";
    } else if (self.isPartiallyMatched(measurementText)) {
      res.matchesTo = "partially-matched";
      res.isPartialMatch = true;
      res.hasErrors(true);
      res.errorMessage = {
        clear: function () {
          this.simpleMessage("");
          this.hasSimpleMessage(false);
          this.showHelpfulMessage(false);
          this.helpfulMessage("");
        },
        simpleMessage: ko.observable(measurementText + " is an incomplete number."),
        hasHelpfulMessage: ko.observable(true),
        showHelpfulMessage: ko.observable(false),
        helpfulMessage: ko.observable(
          "A measurement must be entered in one of the following formats:" + " - Whole number (eg. 24, 127)" + " - Fraction (eg. 1/2, 3/16)" + " - Whole number and Fraction (eg. 1 1/2, 3 3/16)"
        ),
        more: function () {
          this.showHelpfulMessage(true);
        },
        less: function () {
          this.showHelpfulMessage(false);
        }
      };
    } else {
      res.isWellFormed = false;
      res.hasErrors(true);
      res.errorMessage = {
        clear: function () {
          this.simpleMessage("");
          this.hasSimpleMessage(false);
          this.showHelpfulMessage(false);
          this.helpfulMessage("");
        },
        simpleMessage: ko.observable(measurementText + " is not a number."),
        hasHelpfulMessage: ko.observable(true),
        showHelpfulMessage: ko.observable(false),
        helpfulMessage: ko.observable(
          "A measurement must be entered in one of the following formats:" + " - Whole number (eg. 24, 127)" + " - Fraction (eg. 1/2, 3/16)" + " - Whole number and Fraction (eg. 1 1/2, 3 3/16)"
        ),
        more: function () {
          this.showHelpfulMessage(true);
        },
        less: function () {
          this.showHelpfulMessage(false);
        }
      };
    }

    return res;
  }

  self.validateMeasurementString = function (measurementString) {
    var validationResult = self.checkTextFormat(measurementString);

    if (!validationResult.isWellFormed || validationResult.isPartialMatch) {
      validationResult.hasErrors(true);
    }

    return validationResult;
  };
};

windowStore.measurement.measurement = function (dto) {
  var self = this;

  self.init = function (sign, whole, numerator, denominator) {
    if (sign !== 1 && sign !== -1) {
      throw "sign must be 1 or -1";
    }
    if (!(typeof whole === 'number' && (whole === 0 || whole % 1 === 0) &&
        typeof numerator === 'number' && (numerator === 0 || numerator % 1 === 0) &&
        typeof denominator === 'number' && (denominator % 1 === 0))) {
      throw "mixed number components must be integer numbers, and denominator may not be 0."
    }
    if ((denominator !== 1) && (denominator !== 2) && (denominator !== 4) && (denominator !== 8) && (denominator != 16)) {
      throw "fraction is not a multiple of 1/16, or equal to 1.";
    }
    if (whole < 0 || numerator < 0 || denominator < 0) {
      throw "mixed number components must be > 0.";
    }

    self.sign(sign);
    self.whole(whole);
    self.numerator(numerator);
    self.denominator(denominator);
    
    return self;
  };
  self.one = function() {
    self.sign(1);
    self.whole(1);
    self.numerator(0);
    self.denominator(1);
    
    return self;
  }

  self.sign = ko.observable(1);
  self.whole = ko.observable(0);
  self.numerator = ko.observable(0);
  self.denominator = ko.observable(1);

  if (typeof dto !== 'undefined' && dto !== null) {
    self.init(dto.sign(), dto.whole(), dto.numerator(), dto.denominator());
  }

  self.errorMessages = new windowStore.measurement.errorMessages();

  self.getWholeFromString = function (text) {
    var spaceIndex = text.indexOf(" ");
    var wholeString = text;

    if (spaceIndex >= 0) {
      text.substring(0, spaceIndex);
    }

    return parseInt(wholeString);
  };
  self.getNumeratorFromString = function (text) {
    var spaceIndex = text.indexOf(" ");
    var fractString = text.substring(spaceIndex + 1, text.length);
    var vinIndex = fractString.indexOf("/");
    var numString = fractString.substring(0, vinIndex);

    return parseInt(numString);
  };
  self.getDenominatorFromString = function (text) {
    var spaceIndex = text.indexOf(" ");
    var fractString = text.substring(spaceIndex + 1, text.length);
    var vinIndex = fractString.indexOf("/");
    var denomString = fractString.substring(vinIndex + 1, fractString.length);

    return parseInt(denomString);
  };
  self.mapWholeString = function (text) {
    self.sign(1);
    self.whole(self.getWholeFromString(text));
    self.numerator(0);
    self.denominator(1);
  };
  self.mapFractionString = function (text) {
    var nearestSixteenth = self.getNearestSixteenth(
      self.getNumeratorFromString(text),
      self.getDenominatorFromString(text));

    self.sign(1);
    self.whole(nearestSixteenth.whole());
    self.numerator(nearestSixteenth.numerator());
    self.denominator(nearestSixteenth.denominator());
  };
  self.mapWholeAndFractionString = function (text) {
    var nearestSixteenth = self.getNearestSixteenth(
      self.getNumeratorFromString(text),
      self.getDenominatorFromString(text));

    self.sign(1);
    self.whole(self.getWholeFromString(text) + nearestSixteenth.whole());
    self.numerator(nearestSixteenth.numerator());
    self.denominator(nearestSixteenth.denominator());
  };

  self.parse = function (measurementString) {
    var measurementValidator = new windowStore.measurement.measurementValidator();
    var validationResult = measurementValidator.checkTextFormat(measurementString);

    if (!validationResult.isWellFormed) {
      return false;
    }

    if (validationResult.isPartialMatch) {
      return false;
    }

    if (validationResult.matchesTo === measurementValidator.WHOLE_ONLY) {
      self.mapWholeString(measurementString);
      return true;
    } else if (validationResult.matchesTo === measurementValidator.FRACTION_ONLY) {
      self.mapFractionString(measurementString);
      return true;
    } else if (validationResult.matchesTo === measurementValidator.WHOLE_AND_FRACTION) {
      self.mapWholeAndFractionString(measurementString);
      return true;
    }

    return false;
  };

  self.fromDecimal = function (decimal) {
    if (decimal < 0)
      self.sign(-1);
    else
      self.sign(1);

    self.whole(Math.floor(decimal));

    var numSixteenths = Math.round((decimal - self.whole()) / 0.0625);
    var resNum = numSixteenths;
    var resDenom = 16;

    if (numSixteenths === 16) {
      self.whole(self.whole() + 1);
      resNum = 0;
      resDenom = 1;
    } else if (numSixteenths === 8) {
      resNum = 1;
      resDenom = 2;
    } else if (numSixteenths === 4) {
      resNum = 1;
      resDenom = 4;
    } else if (numSixteenths === 2) {
      resNum = 1;
      resDenom = 8;
    } else if (numSixteenths % 16 === 0) {
      resNum = Math.floor(numSixteenths / 8);
      resDenom = 1;
    } else if (numSixteenths % 8 === 0) {
      resNum = Math.floor(numSixteenths / 8);
      resDenom = 2;
    } else if (numSixteenths % 4 === 0) {
      resNum = Math.floor(numSixteenths / 4);
      resDenom = 4;
    } else if (numSixteenths % 2 === 0) {
      resNum = Math.floor(numSixteenths / 2);
      resDenom = 8;
    }

    self.numerator(resNum);
    self.denominator(resDenom);
    
    return self;
  };

  self.getDecimal = function () {
    var nearestsixteenth = self.getNearestSixteenth();
    var res = nearestsixteenth.whole();
    res += (nearestsixteenth.numerator() / nearestsixteenth.denominator());
    return res;
  };

  self.getNearestSixteenth = function (numerator, denominator) {
    var decimal = 0;
    if ((typeof numerator != 'undefined') && (typeof denominator != 'undefined')) {
      decimal = numerator / denominator;
    } else {
      decimal = self.whole() + (self.numerator() / self.denominator());
    }

    var whole = Math.floor(decimal);
    var numSixteenths = Math.round((decimal - whole) / 0.0625);
    var resNum = numSixteenths;
    var resDenom = 16;

    if (numSixteenths === 16) {
      whole += 1;
      resNum = 0;
      resDenom = 1;
    } else if (numSixteenths % 16 === 0) {
      resNum = Math.floor(numSixteenths / 16);
      resDenom = 1;
    } else if (numSixteenths % 8 === 0) {
      resNum = Math.floor(numSixteenths / 8);
      resDenom = 2;
    } else if (numSixteenths % 4 === 0) {
      resNum = Math.floor(numSixteenths / 4);
      resDenom = 4;
    } else if (numSixteenths % 2 === 0) {
      resNum = Math.floor(numSixteenths / 2);
      resDenom = 8;
    }

    var res = new windowStore.measurement.measurement();
    res.sign(self.sign());
    res.whole(whole);
    res.numerator(resNum);
    res.denominator(resDenom);

    return res;
  }

  self.add = function (right) {
    var leftnumerator = (self.whole() * self.denominator()) + self.numerator();
    var leftfraction = new Fraction(leftnumerator, self.denominator());
    var rightnumerator = (right.whole() * right.denominator()) + right.numerator();
    var rightfraction = new Fraction(rightnumerator, right.denominator());
    var sumfraction = leftfraction.add(rightfraction);
    var nearestsixteenth = self.getNearestSixteenth(sumfraction.numerator, sumfraction.denominator);

    var res = new windowStore.measurement.measurement();
    res.sign(1);
    res.whole(nearestsixteenth.whole());
    res.numerator(nearestsixteenth.numerator());
    res.denominator(nearestsixteenth.denominator());

    return res;
  };

  self.subtract = function (right) {
    var leftnumerator = (self.whole() * self.denominator()) + self.numerator();
    var leftfraction = new Fraction(leftnumerator, self.denominator());
    var rightnumerator = (right.whole() * right.denominator()) + right.numerator();
    var rightfraction = new Fraction(rightnumerator, right.denominator());
    var difffraction = leftfraction.subtract(rightfraction);
    var nearestsixteenth = self.getNearestSixteenth(difffraction.numerator, difffraction.denominator);

    var res = new windowStore.measurement.measurement();
    res.sign(1);
    res.whole(nearestsixteenth.whole());
    res.numerator(nearestsixteenth.numerator());
    res.denominator(nearestsixteenth.denominator());

    return res;
  };

  self.multiply = function (right) {
    var leftnumerator = (self.whole() * self.denominator()) + self.numerator();
    var leftfraction = new Fraction(leftnumerator, self.denominator());
    var rightnumerator = (right.whole() * right.denominator()) + right.numerator();
    var rightfraction = new Fraction(rightnumerator, right.denominator());
    var multfraction = leftfraction.multiply(rightfraction);
    var nearestsixteenth = self.getNearestSixteenth(multfraction.numerator, multfraction.denominator);

    var res = new windowStore.measurement.measurement();
    res.sign(1);
    res.whole(nearestsixteenth.whole());
    res.numerator(nearestsixteenth.numerator());
    res.denominator(nearestsixteenth.denominator());

    return res;
  };

  self.divide = function (right) {
    var leftnumerator = (self.whole() * self.denominator()) + self.numerator();
    var leftfraction = new Fraction(leftnumerator, self.denominator());
    var rightnumerator = (right.whole() * right.denominator()) + right.numerator();
    var rightfraction = new Fraction(rightnumerator, right.denominator());
    var quofraction = leftfraction.divide(rightfraction);
    var nearestsixteenth = self.getNearestSixteenth(quofraction.numerator, quofraction.denominator);

    var res = new windowStore.measurement.measurement();
    res.sign(1);
    res.whole(nearestsixteenth.whole());
    res.numerator(nearestsixteenth.numerator());
    res.denominator(nearestsixteenth.denominator());

    return res;
  };

  self.toString = function () {
    var res = self.sign() == 1 ? "" : "-";
    res = res + self.whole() + " " + self.numerator() + "/" + self.denominator();
    return res;
  };

  //
  // required because mapping these measurements to the viewmodels measurements
  // blows up knockout.mapping.fromJS :(
  //

  self.toJS = function () {
    return {
      sign: self.sign(),
      whole: self.whole(),
      numerator: self.numerator(),
      denominator: self.denominator(),
      unitName: "Inches"
    };
  };
}
