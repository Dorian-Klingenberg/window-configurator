const fs = require("fs");
const path = require("path");

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}

function createObservable(initialValue) {
  let value = initialValue;
  const observable = function observable(nextValue) {
    if (arguments.length > 0) {
      value = nextValue;
      return observable;
    }

    return value;
  };

  return observable;
}

global.ko = {
  observable: createObservable,
  mapping: {
    toJS(value) {
      return value;
    }
  }
};
global.windowStore = { order: { item: {} }, measurement: {} };
global.windowStore.measurement.measurementValidator = function measurementValidator() {
  this.checkTextFormat = function checkTextFormat() {
    return { hasErrors() { return false; } };
  };
};
global.windowStore.order.item.PriceCalculator = function PriceCalculator() {
  this.calculatePrice = function calculatePrice() {
    return "$0.00";
  };
};
global.windowStore.order.item.ItemValidator = function ItemValidator() {};

const alerts = [];
global.alert = function alert(message) {
  alerts.push(String(message));
};

const viewModelPath = path.resolve(
  __dirname,
  "..",
  "..",
  "WindowConfigurator",
  "wwwroot",
  "js",
  "windowStore",
  "windowStore.order.item.viewmodel.js"
);
const viewModelSource = fs.readFileSync(viewModelPath, "utf8");
eval(`var windowStore = global.windowStore; var ko = global.ko;${viewModelSource}`);

function setAjaxStub(runBehavior) {
  global.$ = {
    ajax() {
      let doneHandler = null;
      let failHandler = null;
      let pending = null;
      let behaviorExecuted = false;

      function flushPending() {
        if (!pending) {
          return;
        }

        const current = pending;

        if (current.type === "success" && doneHandler) {
          doneHandler(current.result);
          pending = null;
        }

        if (current.type === "fail" && failHandler) {
          failHandler(current.jqXHR, current.textStatus, current.errorThrown);
          pending = null;
        }
      }

      function runBehaviorOnce() {
        if (behaviorExecuted || !runBehavior) {
          return;
        }

        behaviorExecuted = true;
        runBehavior({
          succeed(result) {
            pending = { type: "success", result };
            flushPending();
          },
          fail(jqXHR, textStatus, errorThrown) {
            pending = { type: "fail", jqXHR, textStatus, errorThrown };
            flushPending();
          }
        });
      }

      const chain = {
        done(handler) {
          doneHandler = handler;
          runBehaviorOnce();
          flushPending();
          return chain;
        },
        fail(handler) {
          failHandler = handler;
          runBehaviorOnce();
          flushPending();
          return chain;
        }
      };
      return chain;
    }
  };
}

function createViewModel() {
  const vm = new windowStore.order.item.ItemViewModel();
  vm.orderId = "test-order";
  vm.price = ko.observable("$0.00");
  return vm;
}

function runSuccessCase() {
  alerts.length = 0;
  const vm = createViewModel();
  setAjaxStub(driver => {
    driver.succeed({ authoritativePrice: 591.13 });
  });
  vm.complete();

  assert(vm.price() === "$591.13", `Expected price to be $591.13, got ${vm.price()}`);
  assert(
    alerts.some(message => message.includes("Authoritative price: $591.13")),
    "Expected success alert with authoritative price"
  );
}

function runValidationErrorCase() {
  alerts.length = 0;
  const vm = createViewModel();
  setAjaxStub(driver => {
    driver.fail(
      {
        responseJSON: {
          validationErrors: [
            { field: "frameWidth", message: "frameWidth is above maximum." },
            { field: "sections[0].width", message: "section width is above style maximum." }
          ]
        }
      },
      "Bad Request",
      "Bad Request"
    );
  });
  vm.complete();

  assert(alerts.length > 0, "Expected validation error alert");
  const message = alerts[alerts.length - 1];
  assert(message.includes("Window could not be submitted:"), "Expected submit validation heading");
  assert(message.includes("1. frameWidth is above maximum."), "Expected first numbered validation message");
  assert(message.includes("2. section width is above style maximum."), "Expected second numbered validation message");
}

function runGenericFailureCase() {
  alerts.length = 0;
  const vm = createViewModel();
  setAjaxStub(driver => {
    driver.fail({}, "error", "Submit failed.");
  });
  vm.complete();

  assert(alerts[alerts.length - 1] === "Submit failed.", "Expected generic failure alert");
}

try {
  runSuccessCase();
  runValidationErrorCase();
  runGenericFailureCase();
  process.stdout.write("complete-regression-harness passed\n");
} catch (error) {
  process.stderr.write(`complete-regression-harness failed: ${error.message}\n`);
  process.exit(1);
}
