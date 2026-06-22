import { expect, test } from "@playwright/test";

const europeanStocks = [
  { symbol: "OR.PA", country: "France" },
  { symbol: "ASML.AS", country: "Netherlands" },
  { symbol: "NESN.SW", country: "Switzerland" },
  { symbol: "NOVO-B.CO", country: "Denmark" },
] as const;

test.describe.configure({ mode: "serial" });

for (const stock of europeanStocks) {
  test(`${stock.symbol} (${stock.country}) search and detail data render`, async ({ page }) => {
    await page.goto("/");

    await page.getByLabel("Instrument search").fill(stock.symbol);

    const searchResponse = page.waitForResponse((response) =>
      response.url().includes("/api/instruments/search") &&
      response.url().includes(`query=${encodeURIComponent(stock.symbol)}`) &&
      response.status() === 200
    );

    await page.getByRole("button", { name: "Search" }).click();
    await searchResponse;

    await expect(page.getByText(stock.symbol, { exact: true }).first()).toBeVisible();

    const profileResponse = page.waitForResponse((response) =>
      response.url().includes(`/api/instruments/${encodeURIComponent(stock.symbol)}`) &&
      !response.url().includes("/history") &&
      !response.url().includes("/quote") &&
      response.status() === 200
    );

    const quoteResponse = page.waitForResponse((response) =>
      response.url().includes(`/api/instruments/${encodeURIComponent(stock.symbol)}/quote`) &&
      response.status() === 200
    );

    const historyResponse = page.waitForResponse((response) =>
      response.url().includes(`/api/instruments/${encodeURIComponent(stock.symbol)}/history`) &&
      response.status() === 200
    );

    await page.goto(`/instruments/${encodeURIComponent(stock.symbol)}`);

    await Promise.all([profileResponse, quoteResponse, historyResponse]);

    await expect(page.getByText(stock.symbol, { exact: true }).first()).toBeVisible();
    await expect(page.getByText("Price history")).toBeVisible();
    await expect(page.locator("svg.recharts-surface")).toBeVisible();
    await expect(page.getByText(/Live quote|Latest loaded price/)).toBeVisible();
  });
}
