# Requisition Computer
n14-requisition-paperwork-receiver-name = Logistics Branch
n14-requisition-paperwork-reward-message = Confirmation Received! Transferred ${$amount} from budget surplus

# Requisition Invoice
n14-requisition-paper-print-name = {$name} invoice
n14-requisition-paper-print-manifest = [head=2]
    {$containerName}[/head][bold]{$content}[/bold][head=2]
    WT. {$weight} LBS
    LOT {$lot}
    S/N {$serialNumber}[/head]
n14-requisition-paper-print-content = - {$count} {$item}
