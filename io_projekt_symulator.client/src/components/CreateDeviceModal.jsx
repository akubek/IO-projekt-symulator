import React, { useState } from "react";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import TextField from "@mui/material/TextField";
import FormLabel from "@mui/material/FormLabel";
import RadioGroup from "@mui/material/RadioGroup";
import FormControlLabel from "@mui/material/FormControlLabel";
import Radio from "@mui/material/Radio";
import { Power, Gauge, Activity } from "lucide-react";
import clsx from "clsx";

const deviceTypes = [
    { value: 'switch', label: 'Switch (ON/OFF)', icon: Power, color: 'text-blue-600 bg-blue-50' },
    { value: 'slider', label: 'Slider (Range)', icon: Gauge, color: 'text-purple-600 bg-purple-50' },
    { value: 'sensor', label: 'Sensor ("Read only")', icon: Activity, color: 'text-green-600 bg-green-50' },
];

export default function CreateDeviceModal({ open, onClose, onCreate, isCreating }) {

    // basic device form state
    const [formData, setFormData] = useState({
        name: '',
        type: 'switch',
        location: '',
        description: '',
        initialValue: 0,
        readonly: false,
        min: 0,
        max: 100,
        step: 1,
        unit: '',
    });

    // handling data submission
    const handleSubmit = (e) => {
        e.preventDefault();

        const deviceData = {
            name: formData.name,
            type: formData.type,
            location: formData.location,
            description: formData.description,
            state: {
                value: formData.type === 'switch' ? (formData.initialValue ? 1 : 0) : Number(formData.initialValue),
                unit: formData.type === 'sensor' ? formData.unit : undefined,
            },
            config: {
                readonly: formData.type === 'sensor',
                ...(formData.type !== 'switch' && {
                    min: Number(formData.min),
                    max: Number(formData.max),
                    step: formData.type === 'slider' ? Number(formData.step) : undefined,
                }),
            },
        };

        onCreate(deviceData);
        // Reset form after submission
        setFormData({
            name: '',
            type: formData.type,
            location: '',
            description: '',
            initialValue: 0,
            readonly: false,
            min: 0,
            max: 100,
            step: 1,
            unit: '',
        });
    };

    return (
        <Dialog open={open}>
            <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogTitle className="text-2xl">Create New Device</DialogTitle>

                <form onSubmit={handleSubmit} className="space-y-4">
                    {/* Basic Info */}
                    <div className="space-y-2">
                        <div>
                            <FormLabel htmlFor="name">Device Name *</FormLabel>
                            <TextField
                                id="name"
                                value={formData.name}
                                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                placeholder="e.g., Living Room Light"
                                required
                                className="mt-1.5"
                            />
                        </div>

                        <div>
                            <FormLabel htmlFor="location">Location</FormLabel>
                            <TextField
                                id="location"
                                value={formData.location}
                                onChange={(e) => setFormData({ ...formData, location: e.target.value })}
                                placeholder="e.g., Living Room, Kitchen"
                                className="mt-1.5"
                            />
                        </div>

                        <div>
                            <FormLabel htmlFor="description">Description</FormLabel>
                            <TextField
                                id="description"
                                value={formData.description}
                                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                                placeholder="Optional device description"
                                className="mt-1.5 resize-none"
                                rows={2}
                                multiline={true}
                            />
                        </div>
                    </div>

                    {/* Device Type */}
                    <div className="space-y-5">
                        <FormLabel>Device Type *</FormLabel>
                        <div className="grid grid-cols-3 gap-3">
                            {deviceTypes.map((type) => {
                                const Icon = type.icon;
                                return (
                                    <label key={type.value} className={clsx(
                                            "relative flex flex-col items-center gap-2 p-4 rounded-xl border-2 cursor-pointer transition-all",
                                            formData.type === type.value ? "border-slate-900 bg-slate-50" : "border-slate-200 hover:border-slate-300")}>
                                        <input
                                            type="radio"
                                            name="deviceType"
                                            value={type.value}
                                            checked={formData.type === type.value}
                                            onChange={() => setFormData({ ...formData, type: type.value })}
                                            className="sr-only"/>
                                        <div className={clsx("w-10 h-10 rounded-lg flex items-center justify-center", type.color)}>
                                            <Icon className="w-5 h-5" />
                                        </div>
                                        <span className="text-sm font-medium text-center">{type.label}</span>
                                    </label>
                                );
                            })}
                        </div>
                    </div>

                    {/* Type-Specific Configuration */}
                    {formData.type === 'switch' && (
                        <div className="space-y-4 p-4 bg-blue-50 rounded-xl">
                            <FormLabel className="text-blue-900 flex gap-4">Initial State</FormLabel>
                                <label className="flex items-center gap-2 cursor-pointer">
                                    <input
                                        type="radio"
                                        value="0"
                                        checked={formData.initialValue === 0}
                                        onChange={(e) => setFormData({ ...formData, initialValue: e.target.value === "0" ? 0 : 1 })}
                                        className="cursor-pointer"/>
                                    <span>OFF</span>
                                </label>
                                <label className="flex items-center gap-2 cursor-pointer">
                                    <input
                                        type="radio"
                                        value="1"
                                        checked={formData.initialValue === 1}
                                        onChange={(e) => setFormData({ ...formData, initialValue: e.target.value === "1" ? 1 : 0 })}
                                        className="cursor-pointer"/>
                                    <span>ON</span>
                                </label>
                        </div>
                    )}

                    {formData.type === 'slider' && (
                        <div className="space-y-4 p-4 bg-purple-50 rounded-xl">
                            <FormLabel className="text-purple-900">Slider Configuration</FormLabel>
                            <div className="grid grid-cols-3 gap-3">
                                <div>
                                    <FormLabel htmlFor="min" className="text-sm">Min Value</FormLabel>
                                    <TextField
                                        id="min"
                                        type="number"
                                        value={formData.min}
                                        onChange={(e) => setFormData({ ...formData, min: e.target.value })}
                                        className="mt-1.5"
                                    />
                                </div>
                                <div>
                                    <FormLabel htmlFor="max" className="text-sm">Max Value</FormLabel>
                                    <TextField
                                        id="max"
                                        type="number"
                                        value={formData.max}
                                        onChange={(e) => setFormData({ ...formData, max: e.target.value })}
                                        className="mt-1.5"
                                    />
                                </div>
                                <div>
                                    <FormLabel htmlFor="step" className="text-sm">Step</FormLabel>
                                    <TextField
                                        id="step"
                                        type="number"
                                        value={formData.step}
                                        onChange={(e) => setFormData({ ...formData, step: e.target.value })}
                                        className="mt-1.5"
                                    />
                                </div>
                            </div>
                            <div className="grid grid-cols-1 gap-3">
                                <FormLabel htmlFor="initialValue" className="text-sm">Initial Value</FormLabel>
                                <TextField
                                    id="initialValue"
                                    type="number"
                                    value={formData.initialValue}
                                    onChange={(e) => setFormData({ ...formData, initialValue: e.target.value })}
                                    min={formData.min}
                                    max={formData.max}
                                    className="mt-1.5"
                                />
                            </div>
                        </div>
                    )}

                    {formData.type === 'sensor' && (
                        <div className="space-y-4 p-4 bg-green-50 rounded-xl">
                            <FormLabel className="text-green-900">Sensor Configuration</FormLabel>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <FormLabel htmlFor="sensorMin" className="text-sm">Min Range</FormLabel>
                                    <TextField
                                        id="sensorMin"
                                        type="number"
                                        value={formData.min}
                                        onChange={(e) => setFormData({ ...formData, min: e.target.value })}
                                        className="mt-1.5"
                                    />
                                </div>
                                <div>
                                    <FormLabel htmlFor="sensorMax" className="text-sm">Max Range</FormLabel>
                                    <TextField
                                        id="sensorMax"
                                        type="number"
                                        value={formData.max}
                                        onChange={(e) => setFormData({ ...formData, max: e.target.value })}
                                        className="mt-1.5"
                                    />
                                </div>
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <FormLabel htmlFor="sensorUnit" className="text-sm">Unit</FormLabel>
                                    <TextField
                                        id="sensorUnit"
                                        value={formData.unit}
                                        onChange={(e) => setFormData({ ...formData, unit: e.target.value })}
                                        placeholder="e.g., Celcius, %, lux"
                                        className="mt-1.5"
                                    />
                                </div>
                                <div>
                                    <FormLabel htmlFor="sensorInitial" className="text-sm">Initial Value</FormLabel>
                                    <TextField
                                        id="sensorInitial"
                                        type="number"
                                        value={formData.initialValue}
                                        onChange={(e) => setFormData({ ...formData, initialValue: e.target.value })}
                                        className="mt-1.5"
                                    />
                                </div>
                            </div>
                        </div>
                    )}

                    <DialogActions>
                        <Button type="button" variant="outline" onClick={onClose} disabled={isCreating}>
                            Cancel
                        </Button>
                        <Button
                            type="submit"
                            disabled={isCreating}
                            className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700">
                            {isCreating ? 'Creating...' : 'Create Device'}
                        </Button>
                    </DialogActions>
                </form>
            </DialogContent>
        </Dialog>
    );
}