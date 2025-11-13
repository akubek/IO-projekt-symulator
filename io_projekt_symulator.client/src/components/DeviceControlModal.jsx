import React, { useState, useEffect } from "react";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import Switch from "@mui/material/Switch";
import Slider from "@mui/material/Slider";
import TextField from "@mui/material/TextField";
import FormLabel from "@mui/material/FormLabel";
import Badge from "@mui/material/Badge";
import Divider from "@mui/material/Divider";
import { Power, Gauge, Activity, MapPin, Trash2, Info } from "lucide-react";
import clsx from "clsx";


const deviceTypeConfig = {
    switch: { icon: Power, color: "from-blue-500 to-blue-600", bgLight: "bg-blue-50", textColor: "text-blue-700" },
    slider: { icon: Gauge, color: "from-purple-500 to-purple-600", bgLight: "bg-purple-50", textColor: "text-purple-700" },
    sensor: { icon: Activity, color: "from-green-500 to-green-600", bgLight: "bg-green-50", textColor: "text-green-700" }
};

export default function DeviceControlModal({ device, open, onClose, onUpdate, onDelete, isUpdating, isDeleting }) {

    // local state for device value and delete confirmation dialog state
    const [localValue, setLocalValue] = useState(0);
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

    // sync local value with device state when device changes
    useEffect(() => {
        if (device) {
            setLocalValue(device.state?.value || 0);
        }
    }, [device]);
    if (!device) return null;

    // get config based on device type
    const config = deviceTypeConfig[device.type] || deviceTypeConfig.switch;
    const Icon = config.icon;

    // handlers for different device types

    const handleToggle = (checked) => {
        const newValue = checked ? 1 : 0;
        setLocalValue(newValue);
        onUpdate(device, { ...device.state, value: newValue });
    };

    const handleSliderChange = (value) => {
        setLocalValue(value[0]);
    };

    const handleSliderCommit = (value) => {
        onUpdate(device, { ...device.state, value: value[0] });
    };

    const handleSensorUpdate = () => {
        onUpdate(device, { ...device.state, value: Number(localValue) });
    };

    const handleDelete = () => {
        onDelete();
        setShowDeleteConfirm(false);
    };

    return (
        <>
            <Dialog open={open} onOpenChange={onClose}>
                <DialogContent className="max-w-lg">
                        <div className="flex items-center gap-3 mb-2">
                            <div className={clsx("w-12 h-12 rounded-xl bg-gradient-to-br shadow-lg flex items-center justify-center", config.color)}>
                                <Icon className="w-6 h-6 text-white" />
                            </div>
                            <div className="flex-1">
                                <DialogTitle className="text-xl">{device.name}</DialogTitle>
                                {device.location && (
                                    <div className="flex items-center gap-1 text-sm text-slate-500 mt-1">
                                        <MapPin className="w-3 h-3" />
                                        {device.location}
                                    </div>
                                )}
                            </div>
                            <Badge variant="secondary" className={clsx(config.bgLight, config.textColor)}>
                                {device.type.toUpperCase()}
                            </Badge>
                        </div>
                        {device.description && (
                            <DialogContentText className="text-base">{device.description}</DialogContentText>
                        )}

                    <Divider />

                    {/* Control Section */}
                    <div className="space-y-6 py-4">
                        {/* Switch Control */}
                        {device.type === 'switch' && (
                            <div className={clsx("p-6 rounded-xl", config.bgLight)}>
                                <div className="flex items-center justify-between">
                                    <div>
                                        <FormLabel className="text-lg font-semibold">Device State</FormLabel>
                                        <p className="text-sm text-slate-600 mt-1">Toggle the device on or off</p>
                                    </div>
                                    <div className="flex items-center gap-3">
                                        <span className={clsx("text-lg font-bold", localValue ? "text-green-600" : "text-slate-400")}>
                                            {localValue ? 'ON' : 'OFF'}
                                        </span>
                                        <Switch
                                            checked={localValue > 0}
                                            onCheckedChange={handleToggle}
                                            className="scale-125"
                                            disabled={isUpdating}
                                        />
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Slider Control */}
                        {device.type === 'slider' && (
                            <div className={clsx("p-6 rounded-xl", config.bgLight)}>
                                <div className="space-y-4">
                                    <div className="flex items-end justify-between">
                                        <div>
                                            <FormLabel className="text-lg font-semibold">Control Value</FormLabel>
                                            <p className="text-sm text-slate-600 mt-1">
                                                Range: {device.config?.min || 0} - {device.config?.max || 100}
                                            </p>
                                        </div>
                                        <p className="text-3xl font-bold text-slate-900">{localValue}</p>
                                    </div>
                                    <Slider
                                        value={[localValue]}
                                        onValueChange={handleSliderChange}
                                        onValueCommit={handleSliderCommit}
                                        min={device.config?.min || 0}
                                        max={device.config?.max || 100}
                                        step={device.config?.step || 1}
                                        disabled={isUpdating}
                                        className="cursor-pointer"
                                    />
                                </div>
                            </div>
                        )}

                        {/* Sensor Control */}
                        {device.type === 'sensor' && (
                            <div className={clsx("p-6 rounded-xl", config.bgLight)}>
                                <div className="space-y-4">
                                    <div>
                                        <FormLabel className="text-lg font-semibold">Sensor Reading</FormLabel>
                                        <p className="text-sm text-slate-600 mt-1">
                                            Simulate sensor value (Range: {device.config?.min || 0} - {device.config?.max || 100})
                                        </p>
                                    </div>
                                    <div className="flex items-end gap-3">
                                        <div className="flex-1">
                                            <TextField
                                                type="number"
                                                value={localValue}
                                                onChange={(e) => setLocalValue(e.target.value)}
                                                min={device.config?.min || 0}
                                                max={device.config?.max || 100}
                                                className="text-lg"
                                            />
                                        </div>
                                        {device.state?.unit && (
                                            <div className="text-xl text-slate-600 pb-2">{device.state.unit}</div>
                                        )}
                                        <Button onClick={handleSensorUpdate} disabled={isUpdating} className="px-6">
                                            Update
                                        </Button>
                                    </div>
                                    <div className="h-3 bg-slate-200 rounded-full overflow-hidden">
                                        <div
                                            className={clsx("h-full bg-gradient-to-r transition-all duration-300", config.color)}
                                            style={{ width: `${((localValue || 0) / (device.config?.max || 100)) * 100}%` }}
                                        />
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Device Info */}
                        <div className="p-4 bg-slate-50 rounded-xl space-y-2">
                            <div className="flex items-center gap-2 text-sm text-slate-600">
                                <Info className="w-4 h-4" />
                                <span className="font-medium">Device Information</span>
                            </div>
                            <div className="grid grid-cols-2 gap-2 text-sm ml-6">
                                <div>
                                    <span className="text-slate-500">Device ID:</span>
                                    <p className="font-mono text-xs text-slate-700">{device.id}</p>
                                </div>
                                <div>
                                    <span className="text-slate-500">Created:</span>
                                    <p className="text-slate-700">{new Date(device.created_date).toLocaleDateString()}</p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <Divider />

                    <DialogActions className="flex justify-between sm:justify-between">
                        <Button
                            variant="destructive"
                            onClick={() => setShowDeleteConfirm(true)}
                            disabled={isDeleting}
                            className="gap-2"
                        >
                            <Trash2 className="w-4 h-4" />
                            Delete Device
                        </Button>
                        <Button variant="outline" onClick={onClose}>
                            Close
                        </Button>
                    </DialogActions>
                </DialogContent>
            </Dialog>

            {/* Delete Confirmation */}
            <Dialog open={showDeleteConfirm} onOpenChange={setShowDeleteConfirm}>
                <DialogContent>
                        <DialogTitle>Delete Device?</DialogTitle>
                        <DialogContentText>
                            Are you sure you want to delete "{device.name}"? This action cannot be undone.
                        </DialogContentText>

                    <DialogActions>
                        <Button onClick={() => setShowDeleteConfirm(false)}>Cancel</Button>
                        <Button onClick={handleDelete} className="bg-red-600 hover:bg-red-700">
                            {isDeleting ? 'Deleting...' : 'Delete'}
                        </Button>
                    </DialogActions>
                </DialogContent>
            </Dialog>
        </>
    );
}