import {
    type ColumnDef,
    flexRender,
    getCoreRowModel,
    getPaginationRowModel,
    useReactTable,
} from "@tanstack/react-table"
import { PencilIcon, Trash2Icon } from "lucide-react"
import { type PortfolioTradeResponse } from '@/features/portfolios/details/api/getPortfolio';
import { Button } from "@/components/ui/button"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { deleteTrade as deleteTradeRequest } from "@/features/trades/delete/api/deleteTrade"

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

type PositionTradesDataTableProps = {
    portfolioId: number
    positionId: number
    trades: PortfolioTradeResponse[]
    onSuccess: () => void
}

export default function PositionTradesDataTable({
    portfolioId,
    positionId,
    trades,
    onSuccess
}: PositionTradesDataTableProps) {

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
                        onClick={async () => {
                            await deleteTradeRequest(portfolioId, positionId, row.original.id)
                            onSuccess()
                        }}
                    >
                        <Trash2Icon />
                    </Button>
                </div>
            ),
            enableHiding: false,
        },
    ]


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