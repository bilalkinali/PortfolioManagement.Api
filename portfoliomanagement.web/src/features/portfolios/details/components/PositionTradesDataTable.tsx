import { useState } from "react"
import { Trash2Icon } from "lucide-react"
import { type PortfolioPositionResponse, type PortfolioTradeResponse } from '@/features/portfolios/details/api/getPortfolio';
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { deleteTrade } from "@/features/trades/delete/api/deleteTrade"
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogFooter, DialogTitle } from "@/components/ui/dialog"
import { Spinner } from '@/components/ui/spinner';
import { formatCurrency } from "@/shared/helpers/formatters"
import {
    type ColumnDef,
    flexRender,
    getCoreRowModel,
    getPaginationRowModel,
    useReactTable,
} from "@tanstack/react-table"
import EditTradeDialog from "../../../trades/edit/components/EditTradeDialog";


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
    position: PortfolioPositionResponse
    onSuccess: () => void
}

export default function PositionTradesDataTable({
    portfolioId,
    position,
    onSuccess
}: PositionTradesDataTableProps) {
    const [tradeIdToDelete, setTradeIdToDelete] = useState<number | null>(null)
    const [isDeleting, setIsDeleting] = useState(false)
    const trades = position.trades

    const open = tradeIdToDelete !== null

    async function handleDeleteSubmit() {
        if (tradeIdToDelete === null)
            return;

        setIsDeleting(true);

        try {
            await deleteTrade(portfolioId, position.id, tradeIdToDelete);

            setTradeIdToDelete(null);
            onSuccess();
        } catch (e: unknown) {
            console.error("Error deleting trade:", e);
        } finally {
            setIsDeleting(false);
        }
    }

    function handleCancel() {
        if (isDeleting) return;

        setTradeIdToDelete(null);
    }

    function handleOpenChange(nextOpen: boolean) {
        if (!nextOpen && isDeleting) return;

        if (!nextOpen) {
            setTradeIdToDelete(null);
        }
    }

    const tradeColumns: ColumnDef<PortfolioTradeResponse>[] = [
        {
            accessorKey: "isBuy",
            header: "Side",
            cell: ({ row }) => (
                <Badge
                    variant="outline"
                    className={row.original.isBuy
                        ? "border-green-200 bg-green-50 text-green-700"
                        : "border-red-200 bg-red-50 text-red-700"}
                >
                    {row.original.isBuy ? "Buy" : "Sell"}
                </Badge>
            ),
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
                    {formatCurrency(row.original.price)}
                </div>
            ),
        },
        {
            id: "total",
            header: () => <div className="text-right">Total</div>,
            cell: ({ row }) => (
                <div className="text-right tabular-nums">
                    {formatCurrency(Math.abs(row.original.quantity) * row.original.price)}
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
                    <EditTradeDialog
                        portfolioId={portfolioId}
                        positionId={position.id}
                        trade={row.original}
                        onSuccess={onSuccess}
                    />

                    <Button
                        variant="destructive"
                        size="icon-xs"
                        aria-label={`Delete trade ${row.original.id}`}
                        onClick={() => setTradeIdToDelete(row.original.id)}
                    >
                        <Trash2Icon />
                    </Button>
                </div>
            ),
            enableHiding: false,
        }
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

    const pageCount = table.getPageCount();
    const currentPage = pageCount === 0 ? 0 : table.getState().pagination.pageIndex + 1;
    const showPagination = pageCount > 1;

    return (
        <div className="overflow-hidden rounded-md border bg-background shadow-xs">
            <div className="flex items-center justify-between gap-3 border-b bg-muted/20 px-4 py-3">
                <div>
                    <h3 className="text-sm font-semibold">Trades</h3>
                    <p className="text-xs text-muted-foreground">
                        {position.symbol} transaction history
                    </p>
                </div>

                <Badge variant="outline">
                    {trades.length} trade{trades.length === 1 ? "" : "s"}
                </Badge>
            </div>

            <div>
                <Table className="min-w-[680px]">
                    <TableHeader className="bg-muted/30">
                        {table.getHeaderGroups().map((headerGroup) => (
                            <TableRow key={headerGroup.id} className="hover:bg-muted/50">
                                {headerGroup.headers.map((header) => (
                                    <TableHead key={header.id} className="px-4 text-xs font-semibold text-muted-foreground">
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
                                <TableRow key={row.id} className="hover:bg-muted/30">
                                    {row.getVisibleCells().map((cell) => (
                                        <TableCell key={cell.id} className="px-4 py-3">
                                            {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                        </TableCell>
                                    ))}
                                </TableRow>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell colSpan={tradeColumns.length} className="h-16 text-center text-muted-foreground">
                                    No trades found.
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>

            <div className="flex items-center justify-between gap-3 border-t bg-muted/10 px-4 py-3">
                <p className="text-sm text-muted-foreground">
                    {showPagination
                        ? `Page ${currentPage} of ${pageCount}`
                        : null}
                </p>

                {showPagination ? (
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
                ) : null}
            </div>
            <Dialog open={open} onOpenChange={handleOpenChange}>
                <DialogContent
                    onInteractOutside={(e) => {
                        if (isDeleting) e.preventDefault()
                    }}
                    onEscapeKeyDown={(e) => {
                        if (isDeleting) e.preventDefault()
                    }}
                >
                    <DialogHeader>
                        <DialogTitle>Delete Trade</DialogTitle>
                        <DialogDescription>
                            This action cannot be undone.
                        </DialogDescription>
                    </DialogHeader>

                    <span>
                        Are you sure you want to <span className="font-semibold">delete</span> this trade?
                    </span>

                    <DialogFooter>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={handleCancel}
                            disabled={isDeleting}
                        >
                            Cancel
                        </Button>

                        <Button
                            variant="destructive"
                            type="button"
                            onClick={handleDeleteSubmit}
                            disabled={isDeleting}
                        >
                            {isDeleting ? (
                                <>
                                    Deleting...
                                    <Spinner className="ml-2" />
                                </>
                            ) : (
                                "Delete"
                            )}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </div>
    )
}
