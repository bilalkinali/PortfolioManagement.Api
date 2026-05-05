import * as React from "react"
import {
    type ColumnDef,
    flexRender,
    getCoreRowModel,
    getPaginationRowModel,
    useReactTable,
} from "@tanstack/react-table"
import { ChevronDownIcon, ChevronRightIcon, PencilIcon, Trash2Icon } from "lucide-react"

import { type PortfolioResponse, type PortfolioTradeResponse } from '@/features/portfolios/details/api/getPortfolio';
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import AddTradeDialog from "@/features/trades/add/components/AddTradeDialog"

type PortfolioCardProps = {
    portfolio: PortfolioResponse;
    onSuccess: () => void;
}

const currencyFormatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
})

const numberFormatter = new Intl.NumberFormat("en-US", {
    maximumFractionDigits: 4,
})

const dateFormatter = new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
})

const tradeColumns: ColumnDef<PortfolioTradeResponse>[] = [
    {
        accessorKey: "isBuy",
        header: "Buy / Sell",
        cell: ({ row }) => row.original.isBuy ? "Buy" : "Sell",
    },
    {
        accessorKey: "quantity",
        header: () => <div className="text-right">Quantity</div>,
        cell: ({ row }) => (
            <div className="text-right tabular-nums">
                {numberFormatter.format(row.original.quantity)}
            </div>
        ),
    },
    {
        accessorKey: "price",
        header: () => <div className="text-right">Price</div>,
        cell: ({ row }) => (
            <div className="text-right tabular-nums">
                {currencyFormatter.format(row.original.price)}
            </div>
        ),
    },
    {
        id: "total",
        header: () => <div className="text-right">Total</div>,
        cell: ({ row }) => (
            <div className="text-right tabular-nums">
                {currencyFormatter.format(row.original.quantity * row.original.price)}
            </div>
        ),
    },
    {
        accessorKey: "executedDate",
        header: () => <div className="text-right">Date</div>,
        cell: ({ row }) => (
            <div className="text-right tabular-nums">
                {dateFormatter.format(new Date(row.original.executedDate))}
            </div>
        ),
    },
    {
        id: "actions",
        header: () => <div className="text-right">Actions</div>,
        cell: ({ row }) => (
            <div className="flex justify-end gap-1">
                <Button
                    variant="ghost"
                    size="icon-xs"
                    aria-label={`Edit trade ${row.original.id}`}
                    onClick={() => undefined}
                >
                    <PencilIcon />
                </Button>
                <Button
                    variant="destructive"
                    size="icon-xs"
                    aria-label={`Delete trade ${row.original.id}`}
                    onClick={() => undefined}
                >
                    <Trash2Icon />
                </Button>
            </div>
        ),
        enableHiding: false,
    },
]

function PositionTradesDataTable({ trades }: { trades: PortfolioTradeResponse[] }) {
    const table = useReactTable({
        data: trades,
        columns: tradeColumns,
        getCoreRowModel: getCoreRowModel(),
        getPaginationRowModel: getPaginationRowModel(),
        initialState: {
            pagination: {
                pageSize: 5,
            },
        },
    })
    const pageCount = table.getPageCount()
    const currentPage = pageCount === 0 ? 0 : table.getState().pagination.pageIndex + 1

    return (
        <div className="rounded-b-md border-x border-b bg-background shadow-xs">
            <div className="overflow-hidden">
                <Table>
                    <TableHeader className="bg-muted/50">
                        {table.getHeaderGroups().map((headerGroup) => (
                            <TableRow key={headerGroup.id} className="hover:bg-muted/50">
                                {headerGroup.headers.map((header) => (
                                    <TableHead key={header.id} className="font-semibold">
                                        {header.isPlaceholder
                                            ? null
                                            : flexRender(
                                                header.column.columnDef.header,
                                                header.getContext()
                                            )}
                                    </TableHead>
                                ))}
                            </TableRow>
                        ))}
                    </TableHeader>
                    <TableBody>
                        {table.getRowModel().rows.length ? (
                            table.getRowModel().rows.map((row) => (
                                <TableRow key={row.id}>
                                    {row.getVisibleCells().map((cell) => (
                                        <TableCell key={cell.id}>
                                            {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                        </TableCell>
                                    ))}
                                </TableRow>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell colSpan={tradeColumns.length} className="h-16 text-center">
                                    No trades found.
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>

            <div className="flex items-center justify-between gap-3 border-t bg-muted/20 px-4 py-3">
                <p className="text-sm text-muted-foreground">
                    {trades.length} trade{trades.length === 1 ? "" : "s"} - Page {currentPage} of {pageCount}
                </p>

                <div className="flex items-center gap-2">
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => table.previousPage()}
                        disabled={!table.getCanPreviousPage()}
                    >
                        Previous
                    </Button>
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => table.nextPage()}
                        disabled={!table.getCanNextPage()}
                    >
                        Next
                    </Button>
                </div>
            </div>
        </div>
    )
}

export default function PortfolioCard({ portfolio, onSuccess }: PortfolioCardProps) {
    const [expandedPositionIds, setExpandedPositionIds] = React.useState<Set<number>>(() => new Set())

    function togglePosition(positionId: number) {
        setExpandedPositionIds((current) => {
            const next = new Set(current)

            if (next.has(positionId)) {
                next.delete(positionId)
            } else {
                next.add(positionId)
            }

            return next
        })
    }

    const invested = portfolio.positions.reduce(
        (sum, position) =>
            sum +
            position.trades.reduce(
                (tradeSum, trade) => tradeSum + trade.quantity * trade.price,
                0
            ),
        0
    );

    const marketValue = portfolio.positions.reduce(
        (sum, position) => sum + position.quantity * position.latestPrice,
        0
    );

    const profitLoss = marketValue - invested;

    return (
        <Card className="gap-2">
            <CardHeader>
                <div>
                    <CardTitle>{portfolio.name}</CardTitle>
                    <CardDescription>{portfolio.description}</CardDescription>
                </div>
                <CardAction>
                    <Badge variant="secondary">
                        {new Date(portfolio.createdAt).toDateString()}
                    </Badge>                    
                </CardAction>
            </CardHeader>
            
            <CardContent>
                <div className="grid grid-cols-1 gap-4 pt-4 sm:grid-cols-3 mb-6 text-center">
                    <div className="rounded-md bg-muted p-4">
                        <h1 className="text-sm font-medium text-muted-foreground">Invested</h1>
                        <p className="mt-2 text-xl font-semibold tabular-nums">
                            {currencyFormatter.format(invested)}
                        </p>
                    </div>

                    <div className="rounded-md bg-muted p-4">
                        <h1 className="text-sm font-medium text-muted-foreground">Market Value</h1>
                        <p className="mt-2 text-xl font-semibold tabular-nums">
                            {currencyFormatter.format(marketValue)}
                        </p>
                    </div>

                    <div className="rounded-md bg-muted p-4">
                        <h1 className="text-sm font-medium text-muted-foreground">Profit / Loss</h1>
                        <p className="mt-2 text-xl font-semibold tabular-nums">
                            <span className={
                                profitLoss >= 0
                                    ? "text-green-600"
                                    : "text-red-600"}>
                                {currencyFormatter.format(profitLoss)}
                            </span>
                        </p>
                    </div>
                </div>
                <div className="mb-2 text-right">
                    <AddTradeDialog
                        portfolioId={portfolio.id}
                        onSuccess={onSuccess}
                        buttonVariant="secondary"
                        buttonSize="sm"
                        buttonText="Add Trade" />
                </div>
                <Table className="px-0 min-w-[760px]">
                    <TableHeader className="bg-muted/50">
                        <TableRow className="hover:bg-muted/50">
                            <TableHead className="w-12 px-6">
                                <span className="sr-only">Expand</span>
                            </TableHead>
                            <TableHead className="w-[220px] px-6 font-semibold">Position</TableHead>
                            <TableHead className="text-right font-semibold">Quantity</TableHead>
                            <TableHead className="text-right font-semibold">Avg. Cost</TableHead>
                            <TableHead className="text-right font-semibold">Invested</TableHead>
                            <TableHead className="text-right font-semibold">Market Value</TableHead>
                            <TableHead className="text-right font-semibold">Realized P/L</TableHead>
                            <TableHead className="px-6 text-right font-semibold">Current Price</TableHead>                           
                        </TableRow>
                    </TableHeader>

                    <TableBody>
                        {portfolio.positions.map((position) => {
                            const invested = position.trades.reduce((sum, trade) => sum + trade.quantity * trade.price, 0);
                            const marketValue = position.quantity * position.latestPrice
                            const isProfit = position.realizedPnL >= 0
                            const isExpanded = expandedPositionIds.has(position.id)

                            return (
                                <React.Fragment key={position.id}>
                                    <TableRow>
                                        <TableCell className="px-6">
                                            <Button
                                                variant="ghost"
                                                size="icon-sm"
                                                aria-label={`${isExpanded ? "Collapse" : "Expand"} ${position.symbol} trades`}
                                                aria-expanded={isExpanded}
                                                onClick={() => togglePosition(position.id)}
                                            >
                                                {isExpanded ? (
                                                    <ChevronDownIcon />
                                                ) : (
                                                    <ChevronRightIcon />
                                                )}
                                            </Button>
                                        </TableCell>

                                        <TableCell className="px-6">
                                            <div className="flex flex-col gap-1">
                                                <span className="font-semibold">{position.symbol}</span>
                                                <span className="text-xs text-muted-foreground">
                                                    {position.name}
                                                </span>
                                            </div>
                                        </TableCell>

                                        <TableCell className="text-right tabular-nums">
                                            {numberFormatter.format(position.quantity)}
                                        </TableCell>

                                        <TableCell className="text-right tabular-nums">
                                            {currencyFormatter.format(position.avgCost)}
                                        </TableCell>

                                        <TableCell className="text-right tabular-nums">
                                            {currencyFormatter.format(invested)}
                                        </TableCell>

                                        <TableCell className="text-right tabular-nums">
                                            {currencyFormatter.format(marketValue)}
                                        </TableCell>

                                        <TableCell className="text-right font-medium tabular-nums">
                                            <span className={isProfit ? "text-green-600" : "text-red-600"}>
                                                {currencyFormatter.format(position.realizedPnL)}
                                            </span>
                                        </TableCell>

                                        <TableCell className="px-6 text-right tabular-nums">
                                            <div className="flex flex-col">
                                                <span>
                                                    {position.latestPrice !== null
                                                        ? currencyFormatter.format(position.latestPrice)
                                                        : "—"}
                                                </span>
                                                {position.latestPriceDate ? (
                                                    <span className="text-xs text-muted-foreground">
                                                        {dateFormatter.format(new Date(position.latestPriceDate))}
                                                    </span>
                                                ) : null}
                                            </div>
                                        </TableCell>
                                        
                                    </TableRow>

                                    {isExpanded ? (
                                        <TableRow className="hover:bg-transparent">
                                            <TableCell colSpan={8} className="px-6 pb-4 pt-0">
                                                <div className="ml-16">
                                                    <PositionTradesDataTable trades={position.trades} />
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    ) : null}
                                </React.Fragment>
                            )
                        })}
                    </TableBody>
                </Table>
            </CardContent>
        </Card>
    );
}
