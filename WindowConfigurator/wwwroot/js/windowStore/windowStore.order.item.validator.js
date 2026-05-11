var windowStore = windowStore || {};
windowStore.order = windowStore.order || {};
windowStore.order.item = windowStore.order.item || {};

windowStore.order.item.ItemValidator = function (orderItemViewModel) {
    var self = this;
    self.orderItemViewModel = orderItemViewModel;

    //
    // Section Style to Width Validation
    //
        
    self.sectionWidthTooLargeForStyle = function(style, sectionWidth) {
        var styleWidthMax = new windowStore.measurement.measurement();
        styleWidthMax.init(
            style.restrictions.maxWidth.sign(),
            style.restrictions.maxWidth.whole(),
            style.restrictions.maxWidth.numerator(),
            style.restrictions.maxWidth.denominator()
        );
        
        var styleWidthMaxDecimal = styleWidthMax.getDecimal();
        var sectionWidthDecimal = sectionWidth.getDecimal();
        
        if(sectionWidthDecimal > styleWidthMaxDecimal) {
            return true;
        }
        
        return false;
    }
    self.sectionWidthTooSmallForStyle = function(style, sectionWidth) {
        var styleWidthMin = new windowStore.measurement.measurement();
        styleWidthMin.init(
            style.restrictions.minWidth.sign(),
            style.restrictions.minWidth.whole(),
            style.restrictions.minWidth.numerator(),
            style.restrictions.minWidth.denominator()
        );
        
        var styleWidthMinDecimal = styleWidthMin.getDecimal();
        var sectionWidthDecimal = sectionWidth.getDecimal();
        
        if(sectionWidthDecimal < styleWidthMinDecimal) {
            return true;
        }
        
        return false;
    }
    self.sectionHeightTooLargeForStyle = function(style, sectionHeight) {
        var styleHeightMax = new windowStore.measurement.measurement();
        styleHeightMax.init(
            style.restrictions.maxHeight.sign(),
            style.restrictions.maxHeight.whole(),
            style.restrictions.maxHeight.numerator(),
            style.restrictions.maxHeight.denominator()
        );
        
        var styleHeightMaxDecimal = styleHeightMax.getDecimal();
        var sectionHeightDecimal = sectionHeight.getDecimal();
        
        if(sectionHeightDecimal > styleHeightMaxDecimal) {
            return true;
        }
        
        return false;
    }
    self.sectionHeightTooSmallForStyle = function(style, sectionHeight) {
        var styleHeightMin = new windowStore.measurement.measurement();
        styleHeightMin.init(
            style.restrictions.minHeight.sign(),
            style.restrictions.minHeight.whole(),
            style.restrictions.minHeight.numerator(),
            style.restrictions.minHeight.denominator()
        );
        
        var styleHeightMinDecimal = styleHeightMin.getDecimal();
        var sectionHeightDecimal = sectionHeight.getDecimal();
        
        if(sectionHeightDecimal < styleHeightMinDecimal) {
            return true;
        }
        
        return false;
    }
    
    //
    // Frame width validation
    //
    
    self.frameWidthTooLarge = function(orderItem, frameWidth) {
        var frameWidthMax = new windowStore.measurement.measurement();
        frameWidthMax.init(
            orderItem.productLine.frameRestrictions.maxWidth.sign(),
            orderItem.productLine.frameRestrictions.maxWidth.whole(),
            orderItem.productLine.frameRestrictions.maxWidth.numerator(),
            orderItem.productLine.frameRestrictions.maxWidth.denominator()
        );
        
        var frameWidthMaxDecimal = frameWidthMax.getDecimal();
        var frameWidthDecimal = frameWidth.getDecimal();
        
        if(frameWidthDecimal > frameWidthMaxDecimal) {
            return true;
        }
        
        return false;
    }
    self.frameWidthTooSmall = function(orderItem, frameWidth) {
        var frameWidthMin = new windowStore.measurement.measurement();
        frameWidthMin.init(
            orderItem.productLine.frameRestrictions.minWidth.sign(),
            orderItem.productLine.frameRestrictions.minWidth.whole(),
            orderItem.productLine.frameRestrictions.minWidth.numerator(),
            orderItem.productLine.frameRestrictions.minWidth.denominator()
        );
        
        var frameWidthMinDecimal = frameWidthMin.getDecimal();
        var frameWidthDecimal = frameWidth.getDecimal();
        
        if(frameWidthDecimal < frameWidthMinDecimal) {
            return true;
        }
        
        return false;
    }
    self.frameHeightTooLarge = function(orderItem, frameHeight) {
        var frameHeightMax = new windowStore.measurement.measurement();
        frameHeightMax.init(
            orderItem.productLine.frameRestrictions.maxHeight.sign(),
            orderItem.productLine.frameRestrictions.maxHeight.whole(),
            orderItem.productLine.frameRestrictions.maxHeight.numerator(),
            orderItem.productLine.frameRestrictions.maxHeight.denominator()
        );
        
        var frameHeightMaxDecimal = frameHeightMax.getDecimal();
        var frameHeightDecimal = frameHeight.getDecimal();
        
        if(frameHeightDecimal > frameHeightMaxDecimal) {
            return true;
        }
        
        return false;
    }
    self.frameHeightTooSmall = function(orderItem, frameHeight) {
        var frameHeightMin = new windowStore.measurement.measurement();
        frameHeightMin.init(
            orderItem.productLine.frameRestrictions.minHeight.sign(),
            orderItem.productLine.frameRestrictions.minHeight.whole(),
            orderItem.productLine.frameRestrictions.minHeight.numerator(),
            orderItem.productLine.frameRestrictions.minHeight.denominator()
        );
        
        var frameHeightMinDecimal = frameHeightMin.getDecimal();
        var frameHeightDecimal = frameHeight.getDecimal();
        
        if(frameHeightDecimal < frameHeightMinDecimal) {
            return true;
        }
        
        return false;
    }
};
