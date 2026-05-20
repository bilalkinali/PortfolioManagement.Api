import { Button } from "@/components/ui/button";
import { ButtonGroup } from "@/components/ui/button-group";
import { type StockRange } from "../types/StockRange";

type StockRangeSelectorProps = {
    selectedRange: StockRange;
    onRangeChange: (range: StockRange) => void;
};

const ranges: StockRange[] = ["ALL", "5Y", "1Y", "3M", "1M", "5D"];

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