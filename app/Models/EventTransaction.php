<?php

namespace App;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class EventTransaction extends Model
{
    use SoftDeletes;
    public function Event(){
        return $this->belongsTo(Event::class);
    }
    public function Transaction(){
        return $this->belongsTo(Transaction::class);
    }
}
