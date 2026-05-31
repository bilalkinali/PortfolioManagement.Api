import { Button } from "@/components/ui/button";
import { ButtonGroup } from "@/components/ui/button-group";
import { type StockRange } from "../types/StockRange";

type StockRangeSelectorProps = {
    selectedRange: StockRange;
    onRangeChange: (range: StockRange) => void;
};

const ranges: StockRange[] = ["5D", "1M", "3M", "1Y", "5Y", "ALL"];

export default function StockRangeSelector({
    selectedRange,
    onRangeChange,
}: StockRangeSelectorProps) {
    return (
        <ButtonGroup>
            {ranges.map((range) => (
                <Button
                    key={range}
                    variant={selectedRange === range ? "default" : "outline"}
                    size="xs"
                    onClick={() => onRangeChange(range)}
                >
                    {range}
                </Button>
            ))}
        </ButtonGroup>
    );
}