const fs = require("fs");
const path = require("path");

const repoRoot = path.resolve(__dirname, "..", "..");
const webRoot = path.join(repoRoot, "WindowConfigurator");
const outPath = path.join(__dirname, "pricing-comparison-fixture.json");

global.ko = {
  observable(initial) {
    let value = initial;
    const fn = function (next) {
      if (arguments.length > 0) {
        value = next;
        return fn;
      }

      return value;
    };

    return fn;
  }
};
global.windowStore = {};

eval(fs.readFileSync(path.join(webRoot, "wwwroot", "js", "windowStore", "windowStore.measurement.js"), "utf8"));
eval(fs.readFileSync(path.join(webRoot, "wwwroot", "js", "windowStore", "pricing.js"), "utf8"));

const priceInfo = JSON.parse(fs.readFileSync(path.join(webRoot, "AppData", "priceInfo.json"), "utf8")).priceInfo;
const calculator = new windowStore.order.item.PriceCalculator();

function round4(value) {
  return Math.round(value * 10000) / 10000;
}

function snapToSixteenth(value) {
  return round4(Math.round(value * 16) / 16);
}

function distinctSorted(values) {
  return [...new Set(values)].sort((a, b) => a - b);
}

function measurement(decimalValue) {
  const sixteenths = Math.round(decimalValue * 16);
  const sign = sixteenths < 0 ? -1 : 1;
  const magnitude = Math.abs(sixteenths);
  const whole = Math.floor(magnitude / 16);
  const remainder = magnitude % 16;
  return {
    sign,
    whole,
    numerator: remainder,
    denominator: 16
  };
}

function defaultOption(options, preferredName) {
  return options.find(x => x.name === preferredName) ?? options[0];
}

function priceableBreakpointOptions(options, preferredName) {
  const eligible = options.filter(x => Array.isArray(x.widthBreakpoints) && x.widthBreakpoints.length > 0);
  if (eligible.length > 0) {
    return eligible;
  }

  return [{ name: "__unpriced__" }];
}

function midpoint(values, lowerIndex) {
  const lower = values[Math.max(0, Math.min(values.length - 1, lowerIndex))];
  const upper = values[Math.max(0, Math.min(values.length - 1, lowerIndex + 1))];
  return round4(lower + ((upper - lower) / 2));
}

function exact(values, index) {
  return values[Math.max(0, Math.min(values.length - 1, index))];
}

function buildSizeProfiles(item) {
  const widths = distinctSorted(item.widthBreakpoints.map(x => x.width));
  const representativeHeights = distinctSorted(item.widthBreakpoints[Math.min(item.widthBreakpoints.length - 1, Math.floor(item.widthBreakpoints.length / 2))]
    .heightBreakpoints
    .map(x => x.height));

  const middleWidthIndex = Math.floor(widths.length / 2);
  const middleHeightIndex = Math.floor(representativeHeights.length / 2);
  const lastWidth = widths[widths.length - 1];
  const lastHeight = representativeHeights[representativeHeights.length - 1];
  const widthOverflow = round4(Math.max(1, lastWidth * 0.1));
  const heightOverflow = round4(Math.max(1, lastHeight * 0.1));

  return [
    { label: "min-exact", w: snapToSixteenth(exact(widths, 0)), h: snapToSixteenth(exact(representativeHeights, 0)), past: false },
    { label: "first-midpoint", w: snapToSixteenth(midpoint(widths, 0)), h: snapToSixteenth(midpoint(representativeHeights, 0)), past: false },
    { label: "second-exact", w: snapToSixteenth(exact(widths, 1)), h: snapToSixteenth(exact(representativeHeights, 1)), past: false },
    { label: "second-third-midpoint", w: snapToSixteenth(midpoint(widths, 1)), h: snapToSixteenth(midpoint(representativeHeights, 1)), past: false },
    { label: "middle-exact", w: snapToSixteenth(exact(widths, middleWidthIndex)), h: snapToSixteenth(exact(representativeHeights, middleHeightIndex)), past: false },
    { label: "middle-next-midpoint", w: snapToSixteenth(midpoint(widths, middleWidthIndex)), h: snapToSixteenth(midpoint(representativeHeights, middleHeightIndex)), past: false },
    { label: "penultimate-exact", w: snapToSixteenth(exact(widths, widths.length - 2)), h: snapToSixteenth(exact(representativeHeights, representativeHeights.length - 2)), past: false },
    { label: "max-exact", w: snapToSixteenth(lastWidth), h: snapToSixteenth(lastHeight), past: false },
    { label: "width-overflow", w: snapToSixteenth(lastWidth + widthOverflow), h: snapToSixteenth(lastHeight), past: true },
    { label: "width-height-overflow", w: snapToSixteenth(lastWidth + widthOverflow), h: snapToSixteenth(lastHeight + heightOverflow), past: true }
  ];
}

function getBoundingWidthBreakpoints(item, width) {
  let lowerWidthBreakpoint = item.widthBreakpoints[0];
  let upperWidthBreakpoint = item.widthBreakpoints[0];

  for (const candidate of item.widthBreakpoints) {
    upperWidthBreakpoint = candidate;
    if (width >= upperWidthBreakpoint.width) {
      lowerWidthBreakpoint = upperWidthBreakpoint;
    } else {
      break;
    }
  }

  return {
    lowerWidthBreakpoint,
    upperWidthBreakpoint
  };
}

function isPastBreakpointItem(item, width, height) {
  if (!Array.isArray(item.widthBreakpoints) || item.widthBreakpoints.length === 0) {
    return false;
  }

  const maxWidth = item.widthBreakpoints[item.widthBreakpoints.length - 1].width;
  if (width > maxWidth) {
    return true;
  }

  const { lowerWidthBreakpoint, upperWidthBreakpoint } = getBoundingWidthBreakpoints(item, width);
  const lowerMaxHeight = lowerWidthBreakpoint.heightBreakpoints[lowerWidthBreakpoint.heightBreakpoints.length - 1].height;
  const upperMaxHeight = upperWidthBreakpoint.heightBreakpoints[upperWidthBreakpoint.heightBreakpoints.length - 1].height;

  return height > Math.min(lowerMaxHeight, upperMaxHeight);
}

function parseCurrency(value) {
  return Number.parseFloat(String(value).replace("$", ""));
}

function buildSection(styleName, crankName, grilleName, sdlName, paneName, width, height) {
  return {
    width: measurement(width),
    height: measurement(height),
    style: { name: styleName },
    crank: { name: crankName },
    grillePattern: { name: grilleName },
    sdlPattern: { name: sdlName }
  };
}

function buildWindowModel(productLineName, frameColor, brickmould, paneConfig, sections, frameWidth, frameHeight, outsideWidth, outsideHeight) {
  return {
    productLine: {
      manufacturerName: "All Weather Windows",
      name: productLineName
    },
    frameColor: { name: frameColor },
    brickmouldStyle: { name: brickmould },
    paneConfiguration: { name: paneConfig },
    width: measurement(frameWidth),
    height: measurement(frameHeight),
    osmWidth: measurement(outsideWidth),
    osmHeight: measurement(outsideHeight),
    sections
  };
}

function expectedPrice(windowModel) {
  return round4(parseCurrency(calculator.getPrice(windowModel, priceInfo)));
}

function addCase(cases, record) {
  cases.push(record);
}

function buildSingleSectionCases(productLine, cases) {
    const white = defaultOption(productLine.frameColors, "White").name;
    const noBrickmould = defaultOption(productLine.brickmouldStyles, "None").name;
    const grilleOptions = priceableBreakpointOptions(productLine.grillePatterns, "None");
    const sdlOptions = priceableBreakpointOptions(productLine.sdlPatterns, "None");
    const noGrilleOption = defaultOption(grilleOptions, "None");
    const noSdlOption = defaultOption(sdlOptions, "None");
    const noGrille = noGrilleOption.name;
    const noSdl = noSdlOption.name;
    const noCrank = defaultOption(productLine.cranks, "None").name;

  for (const style of productLine.styles) {
    const sizes = buildSizeProfiles(style);

    for (const frameColor of productLine.frameColors) {
      for (const brickmould of productLine.brickmouldStyles) {
        for (const paneConfig of productLine.paneConfigurations) {
          for (const grille of grilleOptions) {
            for (const sdl of sdlOptions) {
              for (const size of sizes) {
                const section = buildSection(style.name, noCrank, grille.name, sdl.name, paneConfig.name, size.w, size.h);
                const past =
                  isPastBreakpointItem(style, size.w, size.h) ||
                  isPastBreakpointItem(grille, size.w, size.h) ||
                  isPastBreakpointItem(sdl, size.w, size.h) ||
                  isPastBreakpointItem(paneConfig, size.w, size.h) ||
                  isPastBreakpointItem(brickmould, size.w, size.h);
                const model = buildWindowModel(
                  productLine.name,
                  frameColor.name,
                  brickmould.name,
                  paneConfig.name,
                  [section],
                  size.w,
                  size.h,
                  size.w,
                  size.h
                );

                addCase(cases, {
                  id: `${productLine.name}|single|${style.name}|${frameColor.name}|${brickmould.name}|${paneConfig.name}|${grille.name}|${sdl.name}|${size.label}`,
                  pl: productLine.name,
                  sc: 1,
                  past,
                  fw: size.w,
                  fh: size.h,
                  ow: size.w,
                  oh: size.h,
                  fc: frameColor.name,
                  bm: brickmould.name,
                  pn: paneConfig.name,
                  s: [
                    {
                      st: style.name,
                      cr: noCrank,
                      gr: grille.name,
                      sd: sdl.name,
                      pn: paneConfig.name,
                      w: size.w,
                      h: size.h
                    }
                  ],
                  e: expectedPrice(model)
                });
              }
            }
          }
        }
      }
    }

    for (const crank of productLine.cranks) {
      for (const size of sizes) {
        const section = buildSection(style.name, crank.name, noGrille, noSdl, "Dual", size.w, size.h);
        const dualPane = productLine.paneConfigurations.find(x => x.name === "Dual") ?? productLine.paneConfigurations[0];
        const past =
          isPastBreakpointItem(style, size.w, size.h) ||
          isPastBreakpointItem(noGrilleOption, size.w, size.h) ||
          isPastBreakpointItem(noSdlOption, size.w, size.h) ||
          isPastBreakpointItem(dualPane, size.w, size.h);
        const model = buildWindowModel(
          productLine.name,
          white,
          noBrickmould,
          "Dual",
          [section],
          size.w,
          size.h,
          size.w,
          size.h
        );

        addCase(cases, {
          id: `${productLine.name}|single-crank|${style.name}|${crank.name}|${size.label}`,
          pl: productLine.name,
          sc: 1,
          past,
          fw: size.w,
          fh: size.h,
          ow: size.w,
          oh: size.h,
          fc: white,
          bm: noBrickmould,
          pn: "Dual",
          s: [
            {
              st: style.name,
              cr: crank.name,
              gr: noGrille,
              sd: noSdl,
              pn: "Dual",
              w: size.w,
              h: size.h
            }
          ],
          e: expectedPrice(model)
        });
      }
    }
  }
}

function buildMultiSectionCases(productLine, sectionCount, cases) {
  const paneOptions = productLine.paneConfigurations;
  const grilleOptions = priceableBreakpointOptions(productLine.grillePatterns, "None");
  const sdlOptions = priceableBreakpointOptions(productLine.sdlPatterns, "None");
  const crankOptions = productLine.cranks;

  const tuples = [];
  const styles = productLine.styles.map(x => x.name);

  if (sectionCount === 2) {
    for (const left of styles) {
      for (const right of styles) {
        tuples.push([left, right]);
      }
    }
  } else {
    for (const left of styles) {
      for (const center of styles) {
        for (const right of styles) {
          tuples.push([left, center, right]);
        }
      }
    }
  }

  for (const frameColor of productLine.frameColors) {
    for (const brickmould of productLine.brickmouldStyles) {
      for (const tuple of tuples) {
        const tupleProfiles = tuple.map(styleName => {
          const style = productLine.styles.find(x => x.name === styleName);
          return buildSizeProfiles(style);
        });

        for (let profileIndex = 0; profileIndex < 10; profileIndex++) {
          const pane = paneOptions[profileIndex % paneOptions.length];
          const sections = [];
          let outsideWidth = 0;
          let outsideHeight = 0;
          let past = isPastBreakpointItem(brickmould, 0, 0);

          for (let i = 0; i < tuple.length; i++) {
            const profile = tupleProfiles[i][profileIndex];
            const style = productLine.styles.find(x => x.name === tuple[i]);
            const grille = grilleOptions[(profileIndex + i) % grilleOptions.length];
            const sdl = sdlOptions[(profileIndex + i) % sdlOptions.length];
            const crank = crankOptions[(profileIndex + i) % crankOptions.length];

            sections.push({
              st: style.name,
              cr: crank.name,
              gr: grille.name,
              sd: sdl.name,
              pn: pane.name,
              w: profile.w,
              h: profile.h
            });

            outsideWidth = round4(outsideWidth + profile.w);
            outsideHeight = Math.max(outsideHeight, profile.h);
            past =
              past ||
              isPastBreakpointItem(style, profile.w, profile.h) ||
              isPastBreakpointItem(grille, profile.w, profile.h) ||
              isPastBreakpointItem(sdl, profile.w, profile.h) ||
              isPastBreakpointItem(pane, profile.w, profile.h);
          }

          past = past || isPastBreakpointItem(brickmould, outsideWidth, outsideHeight);

          const model = buildWindowModel(
            productLine.name,
            frameColor.name,
            brickmould.name,
            pane.name,
            sections.map(section => buildSection(section.st, section.cr, section.gr, section.sd, section.pn, section.w, section.h)),
            outsideWidth,
            outsideHeight,
            outsideWidth,
            outsideHeight
          );

          addCase(cases, {
            id: `${productLine.name}|multi-${sectionCount}|${tuple.join("+")}|${frameColor.name}|${brickmould.name}|p${profileIndex}`,
            pl: productLine.name,
            sc: sectionCount,
            past,
            fw: outsideWidth,
            fh: outsideHeight,
            ow: outsideWidth,
            oh: outsideHeight,
            fc: frameColor.name,
            bm: brickmould.name,
            pn: pane.name,
            s: sections.map(section => ({
              st: section.st,
              cr: section.cr,
              gr: section.gr,
              sd: section.sd,
              pn: section.pn,
              w: section.w,
              h: section.h
            })),
            e: expectedPrice(model)
          });
        }
      }
    }
  }
}

const manufacturer = priceInfo.manufacturers.find(x => x.name === "All Weather Windows");
const cases = [];

for (const productLine of manufacturer.productLines) {
  buildSingleSectionCases(productLine, cases);
  buildMultiSectionCases(productLine, 2, cases);
  buildMultiSectionCases(productLine, 3, cases);
}

const inRange = cases.filter(x => !x.past).length;
const pastRange = cases.filter(x => x.past).length;

const payload = {
  meta: {
    generatedAtUtc: new Date().toISOString(),
    totalCases: cases.length,
    inRangeCases: inRange,
    pastFinalBreakpointCases: pastRange
  },
  cases
};

fs.writeFileSync(outPath, JSON.stringify(payload));
console.log(`Wrote ${cases.length} pricing comparison cases to ${outPath}`);
