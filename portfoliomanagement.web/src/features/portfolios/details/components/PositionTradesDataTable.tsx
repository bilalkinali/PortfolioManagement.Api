import { useState } from "react"
import { CalendarIcon, PlusIcon, Trash2Icon } from "lucide-react"
import { type PortfolioPositionResponse, type PortfolioTradeResponse, type TradeType } from '@/features/portfolios/details/api/getPortfolio';
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Calendar } from "@/components/ui/calendar"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { deleteTrade } from "@/features/trades/delete/api/deleteTrade"
import { addTrade } from "@/features/trades/add/api/addTrade"
import { editTrade } from "@/features/trades/edit/api/editTrade"
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogFooter, DialogTitle } from "@/components/ui/dialog"
import { Spinner } from '@/components/ui/spinner';
import { formatCurrency, fromDateOnlyString, toDateOnlyString } from "@/shared/helpers/formatters"

type PositionTradesDataTableProps = {
    portfolioId: number
    position: PortfolioPositionResponse
    onSuccess: () => void
}

type TransactionDraft = {
    executedDate: string
    type: TradeType
    shares: string
    price: string
}

const percentFormatter = new Intl.NumberFormat("en-US", {
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
})

function createAddDraft(): TransactionDraft {
    return {
        executedDate: toDateOnlyString(new Date()),
        type: "Buy",
        shares: "",
        price: "",
    }
}

function createTradeDraft(trade: PortfolioTradeResponse): TransactionDraft {
    return {
        executedDate: trade.executedDate,
        type: trade.type,
        shares: trade.shares.toString(),
        price: trade.price.toString(),
    }
}

function getDraftError(draft: TransactionDraft) {
    const shares = Number(draft.shares)
    const price = Number(draft.price)
    const today = toDateOnlyString(new Date())

    if (!draft.executedDate) {
        return "Date is required."
    }

    if (draft.executedDate > today) {
        return "Date cannot be in the future."
    }

    if (draft.type !== "Buy" && draft.type !== "Sell") {
        return "Type must be Buy or Sell."
    }

    if (draft.shares.trim() === "" || !Number.isInteger(shares) || shares <= 0) {
        return "Shares must be a positive whole number."
    }

    if (draft.price.trim() === "" || !Number.isFinite(price) || price <= 0) {
        return "Cost/share must be greater than zero."
    }

    return null
}

function getPnLClassName(value: number | null) {
    if (value == null || value === 0) {
        return "text-muted-foreground"
    }

    return value > 0 ? "text-chart-1" : "text-destructive"
}

function getDraftTotalCost(draft: TransactionDraft) {
    const shares = Number(draft.shares)
    const price = Number(draft.price)

    if (!Number.isFinite(shares) || !Number.isFinite(price) || shares <= 0 || price <= 0) {
        return null
    }

    return shares * price
}

type TradeRealizedGainDisplay = {
    costBasisUsed: number | null
    realizedGain: number | null
    realizedGainPercentage: number | null
}

function getTradeRealizedGainDisplays(trades: PortfolioTradeResponse[]) {
    const displaysByTradeId: Record<number, TradeRealizedGainDisplay> = {}
    let runningQuantity = 0
    let runningCostBasis = 0

    const chronologicalTrades = [...trades].sort((left, right) => {
        const dateComparison = left.executedDate.localeCompare(right.executedDate)

        if (dateComparison !== 0) {
            return dateComparison
        }

        return left.id - right.id
    })

    for (const trade of chronologicalTrades) {
        if (trade.type === "Buy") {
            displaysByTradeId[trade.id] = {
                costBasisUsed: null,
                realizedGain: null,
                realizedGainPercentage: null,
            }

            runningQuantity += trade.shares
            runningCostBasis += trade.shares * trade.price
            continue
        }

        const averageCostBeforeSell = runningQuantity > 0 ? runningCostBasis / runningQuantity : 0
        const costBasisUsed = trade.shares * averageCostBeforeSell
        const sellAmount = trade.shares * trade.price
        const realizedGain = sellAmount - costBasisUsed

        displaysByTradeId[trade.id] = {
            costBasisUsed,
            realizedGain,
            realizedGainPercentage: costBasisUsed > 0 ? (realizedGain / costBasisUsed) * 100 : null,
        }

        runningQuantity -= trade.shares
        runningCostBasis -= costBasisUsed

        if (runningQuantity <= 0) {
            runningQuantity = 0
            runningCostBasis = 0
        }
    }

    return displaysByTradeId
}

function formatDateOnly(value: string) {
    return value.replaceAll("-", "/")
}

type AddTransactionRowProps = {
    draft: TransactionDraft
    currency: string | null
    isSaving: boolean
    onDraftChange: (draft: TransactionDraft) => void
    onSave: () => void
    onCancel: () => void
}

function AddTransactionRow({
    draft,
    currency,
    isSaving,
    onDraftChange,
    onSave,
    onCancel,
}: AddTransactionRowProps) {
    const totalCost = getDraftTotalCost(draft)
    const [datePopoverOpen, setDatePopoverOpen] = useState(false)

    return (
        <TableRow
            onBlurCapture={(event) => {
                const nextFocusedElement = event.relatedTarget

                if (nextFocusedElement instanceof Node && event.currentTarget.contains(nextFocusedElement)) {
                    return
                }

                onSave()
            }}
        >
            <TableCell className="px-2">
                <Popover open={datePopoverOpen} onOpenChange={setDatePopoverOpen}>
                    <PopoverTrigger asChild>
                        <Button
                            type="button"
                            variant="outline"
                            disabled={isSaving}
                            className="w-32 min-w-0 justify-start px-2 text-left font-normal tabular-nums"
                        >
                            <CalendarIcon data-icon="inline-start" />
                            {draft.executedDate ? formatDateOnly(draft.executedDate) : (
                                <span className="text-muted-foreground">YYYY/MM/DD</span>
                            )}
                        </Button>
                    </PopoverTrigger>

                    <PopoverContent className="w-auto p-0" align="start">
                        <Calendar
                            className="rounded-lg border"
                            mode="single"
                            captionLayout="dropdown"
                            selected={fromDateOnlyString(draft.executedDate)}
                            onSelect={(date) => {
                                if (!date) {
                                    return
                                }

                                onDraftChange({ ...draft, executedDate: toDateOnlyString(date) })
                                setDatePopoverOpen(false)
                            }}
                        />
                    </PopoverContent>
                </Popover>
            </TableCell>
            <TableCell className="px-2">
                <select
                    value={draft.type}
                    disabled={isSaving}
                    onChange={(event) => onDraftChange({ ...draft, type: event.target.value as TradeType })}
                    className="border-input bg-transparent text-foreground ring-offset-background focus-visible:ring-ring mx-auto flex h-9 w-20 appearance-none rounded-md border px-2 py-1 text-center text-sm shadow-xs outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50 dark:bg-input/30"
                >
                    <option className="bg-popover text-popover-foreground" value="Buy">Buy</option>
                    <option className="bg-popover text-popover-foreground" value="Sell">Sell</option>
                </select>
            </TableCell>
            <TableCell className="px-2">
                <Input
                    type="number"
                    min="1"
                    value={draft.shares}
                    disabled={isSaving}
                    className="mx-auto w-20 text-right tabular-nums"
                    onChange={(event) => onDraftChange({ ...draft, shares: event.target.value })}
                />
            </TableCell>
            <TableCell className="px-2">
                <Input
                    type="number"
                    min="0.01"
                    step="0.01"
                    value={draft.price}
                    disabled={isSaving}
                    className="mx-auto w-24 text-right tabular-nums"
                    onChange={(event) => onDraftChange({ ...draft, price: event.target.value })}
                />
            </TableCell>
            <TableCell className="px-2 text-right tabular-nums">
                {totalCost !== null ? formatCurrency(totalCost, currency) : "--"}
            </TableCell>
            <TableCell className="px-2 text-right text-muted-foreground">--</TableCell>
            <TableCell className="px-2 text-right text-muted-foreground">--</TableCell>
            <TableCell className="px-2 text-right text-muted-foreground">--</TableCell>
            <TableCell className="w-10 min-w-10 max-w-10 px-1 text-center">
                <div className="flex justify-center">
                    <Button
                        type="button"
                        variant="destructive"
                        size="icon-xs"
                        aria-label="Cancel new transaction"
                        disabled={isSaving}
                        onClick={onCancel}
                    >
                        <Trash2Icon />
                    </Button>
                </div>
            </TableCell>
        </TableRow>
    )
}

type EditableTradeCellsProps = {
    trade: PortfolioTradeResponse
    draft: TransactionDraft
    realizedGainDisplay: TradeRealizedGainDisplay
    currency: string | null
    isSaving: boolean
    onDraftChange: (draft: TransactionDraft) => void
    onSave: (draft: TransactionDraft) => void
    onDelete: () => void
}

function EditableTradeCells({
    trade,
    draft,
    realizedGainDisplay,
    currency,
    isSaving,
    onDraftChange,
    onSave,
    onDelete,
}: EditableTradeCellsProps) {
    const [datePopoverOpen, setDatePopoverOpen] = useState(false)
    const shares = Number(draft.shares)
    const price = Number(draft.price)
    const hasValidShares = Number.isFinite(shares) && shares > 0
    const hasValidPrice = Number.isFinite(price) && price > 0
    const totalCost = hasValidShares && hasValidPrice ? shares * price : trade.totalCost

    return (
        <>
            <TableCell className="px-2">
                <Popover open={datePopoverOpen} onOpenChange={setDatePopoverOpen}>
                    <PopoverTrigger asChild>
                        <Button
                            type="button"
                            variant="outline"
                            disabled={isSaving}
                            className="w-32 min-w-0 justify-start px-2 text-left font-normal tabular-nums"
                        >
                            <CalendarIcon data-icon="inline-start" />
                            {draft.executedDate ? formatDateOnly(draft.executedDate) : (
                                <span className="text-muted-foreground">YYYY/MM/DD</span>
                            )}
                        </Button>
                    </PopoverTrigger>

                    <PopoverContent className="w-auto p-0" align="start">
                        <Calendar
                            className="rounded-lg border"
                            mode="single"
                            captionLayout="dropdown"
                            selected={fromDateOnlyString(draft.executedDate)}
                            onSelect={(date) => {
                                if (!date) {
                                    return
                                }

                                const nextDraft = { ...draft, executedDate: toDateOnlyString(date) }
                                onDraftChange(nextDraft)
                                onSave(nextDraft)
                                setDatePopoverOpen(false)
                            }}
                        />
                    </PopoverContent>
                </Popover>
            </TableCell>
            <TableCell className="px-2">
                <select
                    value={draft.type}
                    disabled={isSaving}
                    onChange={(event) => onDraftChange({ ...draft, type: event.target.value as TradeType })}
                    onBlur={() => onSave(draft)}
                    className="border-input bg-transparent text-foreground ring-offset-background focus-visible:ring-ring mx-auto flex h-9 w-20 appearance-none rounded-md border px-2 py-1 text-center text-sm shadow-xs outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50 dark:bg-input/30"
                >
                    <option className="bg-popover text-popover-foreground" value="Buy">Buy</option>
                    <option className="bg-popover text-popover-foreground" value="Sell">Sell</option>
                </select>
            </TableCell>
            <TableCell className="px-2">
                <Input
                    type="number"
                    min="1"
                    value={draft.shares}
                    disabled={isSaving}
                    className="mx-auto w-20 text-right tabular-nums"
                    onChange={(event) => onDraftChange({ ...draft, shares: event.target.value })}
                    onBlur={() => onSave(draft)}
                />
            </TableCell>
            <TableCell className="px-2">
                <Input
                    type="number"
                    min="0.01"
                    step="0.01"
                    value={draft.price}
                    disabled={isSaving}
                    className="mx-auto w-24 text-right tabular-nums"
                    onChange={(event) => onDraftChange({ ...draft, price: event.target.value })}
                    onBlur={() => onSave(draft)}
                />
            </TableCell>
            <TableCell className="px-2 text-right tabular-nums">
                {formatCurrency(totalCost, currency)}
            </TableCell>
            <TableCell className="px-2 text-right tabular-nums">
                {realizedGainDisplay.costBasisUsed !== null
                    ? formatCurrency(realizedGainDisplay.costBasisUsed, currency)
                    : "--"}
            </TableCell>
            <TableCell className="px-2 text-right tabular-nums">
                <span className={getPnLClassName(realizedGainDisplay.realizedGainPercentage)}>
                    {realizedGainDisplay.realizedGainPercentage !== null
                        ? `${percentFormatter.format(realizedGainDisplay.realizedGainPercentage)}%`
                        : "--"}
                </span>
            </TableCell>
            <TableCell className="px-2 text-right tabular-nums">
                <span className={getPnLClassName(realizedGainDisplay.realizedGain)}>
                    {realizedGainDisplay.realizedGain !== null
                        ? formatCurrency(realizedGainDisplay.realizedGain, currency)
                        : "--"}
                </span>
            </TableCell>
            <TableCell className="w-10 min-w-10 max-w-10 px-1 text-center">
                <div className="flex justify-center">
                    <Button
                        type="button"
                        variant="destructive"
                        size="icon-xs"
                        aria-label={`Delete transaction ${trade.id}`}
                        disabled={isSaving}
                        onClick={onDelete}
                    >
                        <Trash2Icon />
                    </Button>
                </div>
            </TableCell>
        </>
    )
}

export default function PositionTradesDataTable({
    portfolioId,
    position,
    onSuccess
}: PositionTradesDataTableProps) {
    const [tradeIdToDelete, setTradeIdToDelete] = useState<number | null>(null)
    const [isDeleting, setIsDeleting] = useState(false)
    const [isAdding, setIsAdding] = useState(false)
    const [addDraft, setAddDraft] = useState<TransactionDraft>(() => createAddDraft())
    const [isSaving, setIsSaving] = useState(false)
    const [savingTradeId, setSavingTradeId] = useState<number | null>(null)
    const [tradeDrafts, setTradeDrafts] = useState<Record<number, TransactionDraft>>({})
    const [rowError, setRowError] = useState<string | null>(null)
    const trades = position.trades
    const realizedGainDisplaysByTradeId = getTradeRealizedGainDisplays(trades)

    const deleteDialogOpen = tradeIdToDelete !== null

    function startAdd() {
        setRowError(null)
        setAddDraft(createAddDraft())
        setIsAdding(true)
    }

    async function handleAddSubmit() {
        if (addDraft.shares.trim() === "" && addDraft.price.trim() === "") {
            return
        }

        const error = getDraftError(addDraft)

        if (error) {
            setRowError(error)
            return
        }

        setIsSaving(true)
        setRowError(null)

        try {
            await addTrade({
                instrumentId: position.instrumentId,
                type: addDraft.type,
                shares: Number(addDraft.shares),
                price: Number(addDraft.price),
                executedDate: addDraft.executedDate,
            }, portfolioId)

            setIsAdding(false)
            setAddDraft(createAddDraft())
            onSuccess()
        } catch (error) {
            console.error("Add transaction failed", error)
            setRowError(error instanceof Error ? error.message : "Add transaction failed.")
        } finally {
            setIsSaving(false)
        }
    }

    function getTradeDraft(trade: PortfolioTradeResponse) {
        return tradeDrafts[trade.id] ?? createTradeDraft(trade)
    }

    function handleTradeDraftChange(tradeId: number, draft: TransactionDraft) {
        setTradeDrafts((current) => ({
            ...current,
            [tradeId]: draft,
        }))
        setRowError(null)
    }

    async function saveTradeDraft(trade: PortfolioTradeResponse, draft: TransactionDraft) {
        const hasChanges =
            draft.executedDate !== trade.executedDate ||
            draft.type !== trade.type ||
            Number(draft.shares) !== trade.shares ||
            Number(draft.price) !== trade.price

        if (!hasChanges || savingTradeId !== null) {
            return
        }

        const error = getDraftError(draft)

        if (error) {
            setRowError(error)
            return
        }

        setSavingTradeId(trade.id)
        setRowError(null)

        try {
            await editTrade({
                type: draft.type,
                shares: Number(draft.shares),
                price: Number(draft.price),
                executedDate: draft.executedDate,
            }, portfolioId, position.id, trade.id)

            setTradeDrafts((current) => {
                const next = { ...current }
                delete next[trade.id]
                return next
            })
            onSuccess()
        } catch (error) {
            console.error("Edit transaction failed", error)
            setRowError(error instanceof Error ? error.message : "Edit transaction failed.")
        } finally {
            setSavingTradeId(null)
        }
    }

    async function handleDeleteSubmit() {
        if (tradeIdToDelete === null) {
            return
        }

        setIsDeleting(true)

        try {
            await deleteTrade(portfolioId, position.id, tradeIdToDelete)

            setTradeIdToDelete(null)
            onSuccess()
        } catch (error) {
            console.error("Error deleting trade:", error)
        } finally {
            setIsDeleting(false)
        }
    }

    function handleCancelDelete() {
        if (isDeleting) {
            return
        }

        setTradeIdToDelete(null)
    }

    function handleDeleteOpenChange(nextOpen: boolean) {
        if (!nextOpen && isDeleting) {
            return
        }

        if (!nextOpen) {
            setTradeIdToDelete(null)
        }
    }

    return (
        <div className="flex flex-col gap-3 py-3">
            <div className="flex flex-wrap items-center justify-between gap-3">
                <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    disabled={isAdding || isSaving}
                    onClick={startAdd}
                >
                    <PlusIcon data-icon="inline-start" />
                    Add Transaction
                </Button>

                <span className="text-sm text-muted-foreground">
                    {trades.length} transaction{trades.length === 1 ? "" : "s"}
                </span>
            </div>

            <div className="overflow-hidden rounded-md border bg-background">
                <Table className="table-fixed">
                    <colgroup>
                        <col className="w-[14%]" />
                        <col className="w-[9%]" />
                        <col className="w-[9%]" />
                        <col className="w-[11%]" />
                        <col className="w-[13%]" />
                        <col className="w-[13%]" />
                        <col className="w-[13%]" />
                        <col className="w-[14%]" />
                        <col className="w-[4%]" />
                    </colgroup>
                    <TableHeader className="bg-muted/30">
                        <TableRow className="hover:bg-muted/30">
                            <TableHead className="px-2 font-semibold">Date</TableHead>
                            <TableHead className="px-2 text-center font-semibold">Type</TableHead>
                            <TableHead className="px-2 text-center font-semibold">Shares</TableHead>
                            <TableHead className="px-2 text-center font-semibold">Cost/Share</TableHead>
                            <TableHead className="px-2 text-right font-semibold">Amount ($)</TableHead>
                            <TableHead className="px-2 text-right font-semibold">Cost Basis ($)</TableHead>
                            <TableHead className="px-2 text-right font-semibold">Gain (%)</TableHead>
                            <TableHead className="px-2 text-right font-semibold">Gain ($)</TableHead>
                            <TableHead className="w-10 min-w-10 max-w-10 px-1 text-center font-semibold">
                                <span className="sr-only">Actions</span>
                            </TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isAdding ? (
                            <AddTransactionRow
                                draft={addDraft}
                                currency={position.currency}
                                isSaving={isSaving}
                                onDraftChange={(draft) => {
                                    setAddDraft(draft)
                                    setRowError(null)
                                }}
                                onSave={handleAddSubmit}
                                onCancel={() => {
                                    setIsAdding(false)
                                    setRowError(null)
                                }}
                            />
                        ) : null}

                        {trades.length ? (
                            trades.map((trade) => {
                                const draft = getTradeDraft(trade)
                                const rowIsSaving = savingTradeId === trade.id

                                return (
                                    <TableRow key={trade.id}>
                                        <EditableTradeCells
                                            trade={trade}
                                            draft={draft}
                                            realizedGainDisplay={realizedGainDisplaysByTradeId[trade.id]}
                                            currency={position.currency}
                                            isSaving={isSaving || rowIsSaving}
                                            onDraftChange={(nextDraft) => handleTradeDraftChange(trade.id, nextDraft)}
                                            onSave={(nextDraft) => saveTradeDraft(trade, nextDraft)}
                                            onDelete={() => setTradeIdToDelete(trade.id)}
                                        />
                                    </TableRow>
                                )
                            })
                        ) : (
                            <TableRow>
                                <TableCell colSpan={9} className="h-16 text-center text-muted-foreground">
                                    No transactions found.
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>
            {rowError ? (
                <p className="text-sm text-destructive">{rowError}</p>
            ) : null}

            <Dialog open={deleteDialogOpen} onOpenChange={handleDeleteOpenChange}>
                <DialogContent
                    onInteractOutside={(event) => {
                        if (isDeleting) event.preventDefault()
                    }}
                    onEscapeKeyDown={(event) => {
                        if (isDeleting) event.preventDefault()
                    }}
                >
                    <DialogHeader>
                        <DialogTitle>Delete Transaction</DialogTitle>
                        <DialogDescription>
                            This action cannot be undone.
                        </DialogDescription>
                    </DialogHeader>

                    <span>
                        Are you sure you want to <span className="font-semibold">delete</span> this transaction?
                    </span>

                    <DialogFooter>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={handleCancelDelete}
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
